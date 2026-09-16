$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$root = Split-Path -Parent $PSScriptRoot
$jsonPath = Join-Path $root 'Data\Seed\teknofleks-products.json'
$catalog = Get-Content $jsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
$bySlug = @{}
foreach ($p in $catalog) { $bySlug[$p.slug] = $p }

function Decode-Html([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) { return '' }
    $text = [System.Net.WebUtility]::HtmlDecode($value)
    $text = $text -replace '&nbsp;', ' '
    return [regex]::Replace($text, '\s+', ' ').Trim()
}

function Get-Html([string]$url) {
    $tmp = Join-Path $env:TEMP ("tf-" + [guid]::NewGuid().ToString('N') + '.html')
    & curl.exe -sk -L --compressed -A 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)' $url -o $tmp
    $html = [IO.File]::ReadAllText($tmp, [Text.Encoding]::UTF8)
    Remove-Item $tmp -Force
    return $html
}

function Get-PageCount([string]$html) {
    $pages = [regex]::Matches($html, 'urunlerimiz\?[^"]*page=(\d+)') | ForEach-Object { [int]$_.Groups[1].Value }
    if ($pages.Count -eq 0) { return 1 }
    return ($pages | Measure-Object -Maximum).Maximum
}

Write-Host 'Refreshing listing excerpts...'
$first = Get-Html 'https://www.teknofleks.com.tr/tr/urunlerimiz?search='
$totalPages = Get-PageCount $first
for ($page = 1; $page -le $totalPages; $page++) {
    $html = if ($page -eq 1) { $first } else { Get-Html "https://www.teknofleks.com.tr/tr/urunlerimiz?page=$page" }
    $cards = [regex]::Split($html, 'bg-white border rounded-lg shadow-sm') | Select-Object -Skip 1
    foreach ($card in $cards) {
        $href = [regex]::Match($card, 'href="(https://www\.teknofleks\.com\.tr/tr/urun/[^"]+)"').Groups[1].Value
        if (-not $href) { continue }
        $slug = ($href -split '/')[-1]
        $excerpt = Decode-Html([regex]::Match($card, 'text-ellipsis">\s*([^<]+)').Groups[1].Value)
        if ($excerpt -and $bySlug.ContainsKey($slug)) {
            $bySlug[$slug].shortDescription = $excerpt
            if (-not $bySlug[$slug].description -or $bySlug[$slug].description -like '*Kodu okutarak*') {
                $bySlug[$slug].description = $excerpt
            }
        }
    }
    Write-Host "  listing $page / $totalPages"
}

function Get-ProductDescription([string]$html) {
    $best = ''
    foreach ($m in [regex]::Matches($html, '(?s)<p[^>]*>(.*?)</p>')) {
        $t = Decode-Html(([regex]::Replace($m.Groups[1].Value, '<[^>]+>', ' ')))
        if ($t.Length -lt 40 -or $t.Length -gt 2000) { continue }
        if ($t -match 'Kodu okutarak|Ana Sayfa|Teknofleks Endüstri Ürünleri A\.Ş|Sahrayıcedit|Tüm Hakları|Hızlı Linkler') { continue }
        if ($t.Length -gt $best.Length) { $best = $t }
    }
    return $best
}

Write-Host 'Refreshing full descriptions...'
$i = 0
foreach ($p in $catalog) {
    $i++
    try {
        $html = Get-Html ("https://www.teknofleks.com.tr/tr/urun/" + $p.slug)
        $full = Get-ProductDescription $html
        if ($full) { $p.description = $full }
    } catch {
        Write-Host "  fail $($p.slug)"
    }
    if ($i % 25 -eq 0) { Write-Host "  details $i / $($catalog.Count)" }
}

$allowed = @('Çalışma Sıcaklığı','Çalışma Basıncı','Çap','Uzunluk','Menşei','Menşe','Kullanım alanları','Kullanım Alanları','Renkler','Renk','Malzeme')
foreach ($p in $catalog) {
    $clean = @()
    foreach ($f in @($p.features)) {
        $name = ([string]$f.name).Trim()
        $val = ([string]$f.value).Trim()
        if (-not $name -or -not $val) { continue }
        if ($name.Length -lt 3) { continue }
        if ($val -match '^\+90') { continue }
        if ($allowed -notcontains $name) { continue }
        $clean += [ordered]@{ name = $name; value = $val }
    }
    $p.features = $clean
    if ($p.shortDescription -like '*Kodu okutarak*') { $p.shortDescription = $p.description }
    if ($p.shortDescription -and $p.shortDescription.Length -gt 480) {
        $p.shortDescription = $p.shortDescription.Substring(0, 477).Trim() + '...'
    }
}

$catalog | ConvertTo-Json -Depth 6 | Set-Content -Path $jsonPath -Encoding UTF8
$qr = @($catalog | Where-Object { $_.description -like '*Kodu okutarak*' }).Count
$empty = @($catalog | Where-Object { -not $_.description }).Count
Write-Host "Wrote $($catalog.Count) products. QR leftovers=$qr empty=$empty"
