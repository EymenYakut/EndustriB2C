$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root 'Data\Seed'
$imgDir = Join-Path $root 'wwwroot\uploads\products'
New-Item -ItemType Directory -Force -Path $outDir, $imgDir | Out-Null

function Decode-Html([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) { return '' }
    $text = [System.Net.WebUtility]::HtmlDecode($value)
    $text = $text -replace '&nbsp;', ' '
    $text = [regex]::Replace($text, '\s+', ' ').Trim()
    return $text
}

function Get-Html([string]$url) {
    $tmp = Join-Path $env:TEMP ("tf-" + [guid]::NewGuid().ToString('N') + '.html')
    & curl.exe -sk -L --compressed -A 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)' $url -o $tmp
    if ($LASTEXITCODE -ne 0) { throw "Fetch failed: $url" }
    $html = [IO.File]::ReadAllText($tmp, [Text.Encoding]::UTF8)
    Remove-Item $tmp -Force
    return $html
}

function Parse-Cards([string]$html) {
    $cards = [regex]::Split($html, 'bg-white border rounded-lg shadow-sm') | Select-Object -Skip 1
    $items = @()
    foreach ($card in $cards) {
        $href = [regex]::Match($card, 'href="(https://www\.teknofleks\.com\.tr/tr/urun/[^"]+)"').Groups[1].Value
        if (-not $href) { continue }
        $slug = ($href -split '/')[-1]

        $img = [regex]::Match($card, 'src="(https://www\.teknofleks\.com\.tr/storage/products/[^"]+)"').Groups[1].Value
        $name = Decode-Html([regex]::Match($card, 'hover:text-blue-600">([^<]+)').Groups[1].Value)
        $sku = Decode-Html([regex]::Match($card, 'Kod:\s*([^<]+)').Groups[1].Value)
        $desc = Decode-Html([regex]::Match($card, 'text-ellipsis">\s*([^<]+)').Groups[1].Value)

        $features = [ordered]@{}
        foreach ($m in [regex]::Matches($card, '<strong>\s*([^:<]+)\s*:</strong>\s*([^<]*)')) {
            $key = Decode-Html($m.Groups[1].Value)
            $val = Decode-Html($m.Groups[2].Value)
            if ($key) { $features[$key] = $val }
        }

        $origins = @()
        foreach ($m in [regex]::Matches($card, 'images/flags/[^"]+"[^>]*title="([^"]+)"')) {
            $t = Decode-Html($m.Groups[1].Value)
            if ($t -and $origins -notcontains $t) { $origins += $t }
        }

        $usages = @()
        foreach ($m in [regex]::Matches($card, 'rounded-full"[^>]*title="([^"]+)"')) {
            $t = Decode-Html($m.Groups[1].Value)
            if ($t -and $usages -notcontains $t) { $usages += $t }
        }

        $colors = @()
        foreach ($m in [regex]::Matches($card, 'rounded-md" title="([^"]+)"')) {
            $t = Decode-Html($m.Groups[1].Value)
            if ($t -and $colors -notcontains $t) { $colors += $t }
        }

        if ($origins.Count) { $features['Menşei'] = ($origins -join ', ') }
        if ($usages.Count) { $features['Kullanım alanları'] = ($usages -join ', ') }
        if ($colors.Count) { $features['Renkler'] = ($colors -join ', ') }

        $items += [pscustomobject]@{
            slug        = $slug
            url         = $href
            name        = $name
            sku         = $sku
            description = $desc
            imageUrl    = $img
            features    = $features
            origins     = $origins
            usages      = $usages
            colors      = $colors
        }
    }
    return $items
}

function Get-PageCount([string]$html) {
    $pages = [regex]::Matches($html, 'urunlerimiz\?[^"]*page=(\d+)') | ForEach-Object { [int]$_.Groups[1].Value }
    if ($pages.Count -eq 0) { return 1 }
    return ($pages | Measure-Object -Maximum).Maximum
}

Write-Host 'Listing pages...'
$all = @{}
$first = Get-Html 'https://www.teknofleks.com.tr/tr/urunlerimiz?search='
$totalPages = Get-PageCount $first
Write-Host "Found $totalPages listing pages"

for ($page = 1; $page -le $totalPages; $page++) {
    $html = if ($page -eq 1) { $first } else { Get-Html "https://www.teknofleks.com.tr/tr/urunlerimiz?page=$page" }
    $parsed = Parse-Cards $html
    foreach ($p in $parsed) {
        if (-not $all.ContainsKey($p.slug)) { $all[$p.slug] = $p }
    }
    Write-Host "  page $page : $($parsed.Count) cards, unique $($all.Count)"
}

$subSlugs = @(
    'kompozit-akaryakit-hortumlari','kompozit-kimyasal-hortumlar','kompozit-kriyojenik-hortumlar','kompozit-hortum-aksesuarlari',
    'termoflex-yuksek-isi-hortumlari','poliuretan-flexible-hortumlar','cok-amacli-hortumlar-tel-takviyeli','cok-amacli-hortumlar-sert-spiral-takviyeli','cok-amacli-hortumlar-orgu-takviyeli','pu-granul-toz-emis-hortumlari','vakum-emis-hortumlari','yat-hortumlari','yuzme-havuzu-hortumlari','garaj-egzos-hortumlari','teflon-hortumlar','paslanmaz-flexible-hortumlar',
    'su-hortumlari','hava-hortumlari','gida-hortumlari','akaryakit-hortumlari','buhar-hortumlari','kum-ve-camur-hortumlari','kimyasal-hortumlar','kaucuk-yangin-hortumlari',
    'telli-seffaf-hortumlar','spiral-emici-verici-hortumlar','orgulu-sanayi-insaat-ve-tarim-hortumlari','yassi-verici-sulama-hortumlari','duz-seffaf-hortumlar','pvc-yangin-hortumlari',
    'kaucuk-hidrolik-hortumlar','termoplastik-hidrolik-hortumlar','spiral-korumalar',
    'poliuretan-pu-hortumlar','poliamid-pa-hortumlar','polietilen-pe-hortumlar','silikon-hortumlar',
    'havalandirma-ve-duman-emis-hortumlari','agac-sanayi-ve-toz-emis-hortumlari',
    'paslanmaz-kamloklar','poliproplene-kamloklar','aluminyum-kamloklar','takviyeli-agir-is-hortum-kelepceleri','ayarli-hortum-kelepceleri','lock-tipi-kelepceler','paslanmaz-fittings',
    'pvc-serit-perdeler','pvc-levhalar'
)
$mainSlugs = @(
    'gassoflex-kompozit-hortumlar','ithal-endustriyel-hortum','kaucuk-hortum','pvc-hortum','hidrolik-hortum','pnomatik-hortum',
    'fleksible-toz-hava-duman-emis-ve-havalandirma-hortumu','hortum-baglanti-elemanlari','pvc-serit-perde-ve-levha'
)

$categoryBySlug = @{}
Write-Host 'Category pages...'
foreach ($slug in $subSlugs) {
    $html = Get-Html "https://www.teknofleks.com.tr/tr/urunlerimiz?category=$slug"
    $pages = Get-PageCount $html
    for ($page = 1; $page -le $pages; $page++) {
        $pageHtml = if ($page -eq 1) { $html } else { Get-Html "https://www.teknofleks.com.tr/tr/urunlerimiz?category=$slug&page=$page" }
        foreach ($p in (Parse-Cards $pageHtml)) {
            if (-not $categoryBySlug.ContainsKey($p.slug)) { $categoryBySlug[$p.slug] = $slug }
        }
    }
    Write-Host "  $slug mapped $($categoryBySlug.Count)"
}

foreach ($slug in $mainSlugs) {
    $html = Get-Html "https://www.teknofleks.com.tr/tr/urunlerimiz?category=$slug"
    $pages = Get-PageCount $html
    for ($page = 1; $page -le $pages; $page++) {
        $pageHtml = if ($page -eq 1) { $html } else { Get-Html "https://www.teknofleks.com.tr/tr/urunlerimiz?category=$slug&page=$page" }
        foreach ($p in (Parse-Cards $pageHtml)) {
            if (-not $categoryBySlug.ContainsKey($p.slug)) { $categoryBySlug[$p.slug] = $slug }
        }
    }
}

Write-Host 'Detail pages for full descriptions...'
$i = 0
foreach ($p in @($all.Values)) {
    $i++
    try {
        $detail = Get-Html $p.url
        $full = Decode-Html([regex]::Match($detail, '(?s)Ürün Kodu:.*?</h1>\s*<.*?</div>\s*<p[^>]*>\s*(.*?)</p>').Groups[1].Value)
        if (-not $full) {
            $candidates = [regex]::Matches($detail, '(?s)<p class="[^"]*text-gray[^"]*"[^>]*>(.*?)</p>')
            foreach ($c in $candidates) {
                $t = Decode-Html(($c.Groups[1].Value -replace '<[^>]+>', ' '))
                if ($t.Length -gt 40) { $full = $t; break }
            }
        }
        if ($full.Length -gt 20) { $p.description = $full }
    } catch {
        Write-Host "  detail failed $($p.slug): $_"
    }
    if ($i % 20 -eq 0) { Write-Host "  details $i / $($all.Count)" }
}

Write-Host 'Images...'
$usedSku = @{}
$catalog = @()
$sort = 1
foreach ($p in ($all.Values | Sort-Object name)) {
    $sku = if ($p.sku) { $p.sku } else { $p.slug }
    $sku = $sku.Trim()
    if ($sku.Length -gt 50) { $sku = $sku.Substring(0, 50).Trim() }
    $base = $sku
    $n = 2
    while ($usedSku.ContainsKey($sku)) {
        $suffix = "-$n"
        $sku = if (($base.Length + $suffix.Length) -gt 50) { $base.Substring(0, 50 - $suffix.Length) + $suffix } else { $base + $suffix }
        $n++
    }
    $usedSku[$sku] = $true

    $localImage = $null
    if ($p.imageUrl) {
        $ext = [IO.Path]::GetExtension(($p.imageUrl -split '\?')[0])
        if (-not $ext) { $ext = '.jpg' }
        $file = "$($p.slug)$ext".ToLowerInvariant()
        $dest = Join-Path $imgDir $file
        if (-not (Test-Path $dest)) {
            & curl.exe -sk -L --compressed -A 'Mozilla/5.0' $p.imageUrl -o $dest
        }
        if ((Test-Path $dest) -and ((Get-Item $dest).Length -gt 500)) {
            $localImage = "/uploads/products/$file"
        }
    }

    $feats = @()
    foreach ($k in $p.features.Keys) {
        $val = [string]$p.features[$k]
        if ([string]::IsNullOrWhiteSpace($val)) { continue }
        if ($val.Length -gt 300) { $val = $val.Substring(0, 300) }
        $feats += [ordered]@{ name = $k; value = $val }
    }

    $short = $p.description
    if ($short.Length -gt 480) { $short = $short.Substring(0, 477).Trim() + '...' }

    $catalog += [ordered]@{
        sku              = $sku
        slug             = $p.slug
        name             = $p.name
        shortDescription = $short
        description      = $p.description
        categorySlug     = $(if ($categoryBySlug.ContainsKey($p.slug)) { $categoryBySlug[$p.slug] } else { '' })
        imagePath        = $localImage
        imageUrl         = $p.imageUrl
        features         = $feats
        sortOrder        = $sort
    }
    $sort++
}

$jsonPath = Join-Path $outDir 'teknofleks-products.json'
$catalog | ConvertTo-Json -Depth 6 | Set-Content -Path $jsonPath -Encoding UTF8
Write-Host "Wrote $($catalog.Count) products to $jsonPath"
Write-Host "With category: $(($catalog | Where-Object { $_.categorySlug }).Count)"
Write-Host "With image: $(($catalog | Where-Object { $_.imagePath }).Count)"
