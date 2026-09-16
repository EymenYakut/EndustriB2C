using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EndustriB2C.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            await SeedCoreAsync(db);
        }

        await EnsureCategoriesAsync(db);
        await EnsureSlidersAsync(db);
    }

    private static async Task SeedCoreAsync(AppDbContext db)
    {
        var hasher = new PasswordHasher<User>();
        var admin = new User
        {
            Email = "admin@endustri.com",
            Phone = "02164285381",
            FirstName = "Sistem",
            LastName = "Yöneticisi",
            UserType = UserType.Admin,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        var demoUser = new User
        {
            Email = "musteri@endustri.com",
            Phone = "05321234567",
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            UserType = UserType.User,
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        demoUser.PasswordHash = hasher.HashPassword(demoUser, "User123!");
        demoUser.Addresses.Add(new UserAddress
        {
            Title = "İş",
            FullName = "Ahmet Yılmaz",
            Phone = "05321234567",
            City = "İstanbul",
            District = "Kadıköy",
            AddressLine = "Sahrayıcedit Mah. Atatürk Cad. No: 69 D:100",
            PostalCode = "34734",
            IsDefault = true
        });

        db.Users.AddRange(admin, demoUser);

        var headers = DefaultFeatureHeaders();
        db.ProductFeaturesHeaders.AddRange(headers);

        var campaign = new Campaign
        {
            Name = "Endüstri Açılış Kampanyası",
            Description = "Seçili hortum ve bağlantı elemanlarında %15 indirim.",
            DiscountType = DiscountType.Percent,
            DiscountValue = 15,
            CouponCode = "ENDUSTRI15",
            MinOrderAmount = 1000,
            StartDate = DateTimeOffset.UtcNow.AddDays(-1),
            EndDate = DateTimeOffset.UtcNow.AddMonths(2),
            IsActive = true
        };
        db.Campaigns.Add(campaign);

        var products = new[]
        {
            CreateProduct("PVC-CT-001", "pvc-kauçuk", "SEL Hortum", 120, 1, "Çelik Telli Şeffaf Hortum",
                "Gıda ve sıvı transferinde kullanılan şeffaf çelik telli PVC hortum.",
                "İçme suyu, gıda ve genel amaçlı sıvı transferi için çelik helezon telli, esnek ve şeffaf PVC hortum.",
                185, campaign,
                ("Çap", "25-150"), ("Çalışma Basıncı", "6-10"), ("Çalışma Sıcaklığı", "-10 / +60"), ("Malzeme", "PVC"), ("Renk", "Şeffaf"), ("Menşe", "Türkiye")),
            CreateProduct("KOM-001", "kompozit", "GASSO", 40, 2, "Kompozit Hortum",
                "Akaryakıt, kimyasal, LPG ve buhar transferi için hafif kompozit hortum.",
                "İçleri PP veya PTFE film tabakaları ile doldurulmuş, dış yüzeyi PVC sargılı, iç ve dış galvaniz/paslanmaz tel takviyeli kompozit hortum. Kauçuk muadillerine göre çok daha hafif ve esnektir.",
                2450, campaign,
                ("Çap", "25-250"), ("Çalışma Basıncı", "10-14"), ("Çalışma Sıcaklığı", "-30 / +100"), ("Malzeme", "PP / PTFE / PVC"), ("Renk", "Beyaz, Lacivert, Kırmızı, Yeşil"), ("Menşe", "İspanya")),
            CreateProduct("HID-001", "hidrolik", "Continental AG", 80, 3, "Hidrolik Hortum",
                "Yüksek basınçlı hidrolik sistemler için çelik örgülü hortum.",
                "Endüstriyel makineler ve mobil hidrolik sistemlerde kullanılan yüksek basınç dayanımlı, çelik örgülü hidrolik hortum.",
                420, null,
                ("Çap", "6-51"), ("Çalışma Basıncı", "150-400"), ("Çalışma Sıcaklığı", "-40 / +100"), ("Malzeme", "Sentetik Kauçuk"), ("Renk", "Siyah"), ("Menşe", "Almanya")),
            CreateProduct("PVC-SP-001", "pvc-serit", "Extruflex", 200, 4, "PVC Şerit Perde",
                "Soğuk hava, toz ve haşere kontrolü için PVC şerit perde.",
                "Endüstriyel tesislerde enerji tasarrufu, hijyen ve operasyonel verimlilik sağlayan esnek PVC şerit perde sistemleri.",
                95, campaign,
                ("Çap", "-"), ("Çalışma Basıncı", "-"), ("Çalışma Sıcaklığı", "-25 / +50"), ("Malzeme", "PVC"), ("Renk", "Şeffaf / Amber"), ("Menşe", "Fransa")),
            CreateProduct("KAM-AL-001", "kamlok", "Tecnica S.r.L", 300, 5, "Alüminyum Kamlok",
                "Hızlı ve aletsiz hortum bağlantısı için alüminyum kamlok.",
                "Sıvı, toz ve gaz transferlerinde kullanılan camlock (kamlok) hızlı bağlantı sistemi. Alüminyum gövde, paslanmaz kilit kolları.",
                310, null,
                ("Çap", "15-150"), ("Çalışma Basıncı", "10"), ("Çalışma Sıcaklığı", "-20 / +80"), ("Malzeme", "Alüminyum"), ("Renk", "Gümüş"), ("Menşe", "İtalya")),
            CreateProduct("PVC-YE-001", "pvc-kauçuk", "SEL Hortum", 90, 6, "Yeşil Emici Hortum",
                "Aşındırıcı malzeme ve su emişi için spiral telli emici hortum.",
                "Tarım, inşaat ve sanayide su, çamur ve hafif aşındırıcı malzemelerin emiş/tahliye işlerinde kullanılan yeşil spiral hortum.",
                165, null,
                ("Çap", "32-200"), ("Çalışma Basıncı", "3-6"), ("Çalışma Sıcaklığı", "-10 / +55"), ("Malzeme", "PVC"), ("Renk", "Yeşil"), ("Menşe", "Türkiye"))
        };

        foreach (var (product, featurePairs) in products)
        {
            foreach (var (name, value) in featurePairs)
            {
                var header = headers.First(h => h.Name == name);
                product.Features.Add(new ProductFeaturesDetail
                {
                    Header = header,
                    Value = value
                });
            }
            db.Products.Add(product);
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureCategoriesAsync(AppDbContext db)
    {
        await EnsureTeknofleksCatalogAsync(db);
        await EnsureFeatureHeadersAsync(db);
        if (TryFindCatalogPath() is { } catalogPath)
        {
            await ImportTeknofleksProductsAsync(db, catalogPath);
        }
        else
        {
            await EnsureExtraProductsAsync(db);
            await BindProductsToSubcategoriesAsync(db);
        }
    }

    private static async Task EnsureSlidersAsync(AppDbContext db)
    {
        var defaults = DefaultSlides();
        var existing = await db.HomeSliders.OrderBy(s => s.SortOrder).ThenBy(s => s.Id).ToListAsync();

        if (existing.Count == 0)
        {
            db.HomeSliders.AddRange(defaults);
            await db.SaveChangesAsync();
            return;
        }

        if (existing.All(s => string.IsNullOrWhiteSpace(s.ImageUrl)))
        {
            db.HomeSliders.RemoveRange(existing);
            db.HomeSliders.AddRange(defaults);
            await db.SaveChangesAsync();
        }
    }

    private static HomeSlider[] DefaultSlides() =>
    [
        new HomeSlider
        {
            Title = "Kompozit hortumlar",
            Subtitle = "Akaryakıt, kimyasal ve kriyojenik hatlar. GASSOFLEX stoktan teslim.",
            ButtonText = "Ürünleri incele",
            ButtonUrl = "/urunler",
            ImageUrl = "/img/slides/01-kompozit.jpg",
            SortOrder = 1,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        },
        new HomeSlider
        {
            Title = "Kauçuk sanayi hortumları",
            Subtitle = "Hava, su ve basınç hortumları. SEL ve Avrupa markaları.",
            ButtonText = "Katalog",
            ButtonUrl = "/urunler",
            ImageUrl = "/img/slides/02-kaucuk.jpg",
            SortOrder = 2,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        },
        new HomeSlider
        {
            Title = "PVC emici hortumlar",
            Subtitle = "Spiral takviyeli emme ve basma hortumları. Şantiye ve tesis hatları.",
            ButtonText = "Ürünleri incele",
            ButtonUrl = "/urunler",
            ImageUrl = "/img/slides/03-emici.jpg",
            SortOrder = 3,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        },
        new HomeSlider
        {
            Title = "Makara ve bağlantı",
            Subtitle = "Hortum makarası, kamlok, kelepçe ve kaplin. Pres ve montaj.",
            ButtonText = "Katalog",
            ButtonUrl = "/urunler",
            ImageUrl = "/img/slides/04-makara.jpg",
            SortOrder = 4,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        }
    ];

    private static async Task EnsureTeknofleksCatalogAsync(AppDbContext db)
    {
        var tree = new (string Name, string Slug, (string Name, string Slug)[] Subs)[]
        {
            ("GASSOFLEX KOMPOZİT HORTUMLAR", "gassoflex-kompozit-hortumlar", new[]
            {
                ("KOMPOZİT AKARYAKIT HORTUMLARI", "kompozit-akaryakit-hortumlari"),
                ("KOMPOZİT KİMYASAL HORTUMLAR", "kompozit-kimyasal-hortumlar"),
                ("KOMPOZİT KRİYOJENİK HORTUMLAR", "kompozit-kriyojenik-hortumlar"),
                ("KOMPOZİT HORTUM AKSESUARLARI", "kompozit-hortum-aksesuarlari")
            }),
            ("İTHAL ENDÜSTRİYEL HORTUM", "ithal-endustriyel-hortum", new[]
            {
                ("TERMOFLEX YÜKSEK ISI HORTUMLARI", "termoflex-yuksek-isi-hortumlari"),
                ("POLİÜRETAN FLEXIBLE HORTUMLAR", "poliuretan-flexible-hortumlar"),
                ("ÇOK AMAÇLI HORTUMLAR - TEL TAKVİYELİ", "cok-amacli-hortumlar-tel-takviyeli"),
                ("ÇOK AMAÇLI HORTUMLAR - SERT SPİRAL TAKVİYELİ", "cok-amacli-hortumlar-sert-spiral-takviyeli"),
                ("ÇOK AMAÇLI HORTUMLAR-ÖRGÜ TAKVİYELİ", "cok-amacli-hortumlar-orgu-takviyeli"),
                ("PU GRANÜL TOZ EMİŞ HORTUMLARI", "pu-granul-toz-emis-hortumlari"),
                ("VAKUM EMİŞ HORTUMLARI", "vakum-emis-hortumlari"),
                ("YAT HORTUMLARI", "yat-hortumlari"),
                ("YÜZME HAVUZU HORTUMLARI", "yuzme-havuzu-hortumlari"),
                ("GARAJ EGZOS HORTUMLARI", "garaj-egzos-hortumlari"),
                ("TEFLON HORTUMLAR", "teflon-hortumlar"),
                ("PASLANMAZ FLEXIBLE HORTUMLAR", "paslanmaz-flexible-hortumlar")
            }),
            ("KAUÇUK HORTUM", "kaucuk-hortum", new[]
            {
                ("SU HORTUMLARI", "su-hortumlari"),
                ("HAVA HORTUMLARI", "hava-hortumlari"),
                ("GIDA HORTUMLARI", "gida-hortumlari"),
                ("AKARYAKIT HORTUMLARI", "akaryakit-hortumlari"),
                ("BUHAR HORTUMLARI", "buhar-hortumlari"),
                ("KUM VE ÇAMUR HORTUMLARI", "kum-ve-camur-hortumlari"),
                ("KİMYASAL HORTUMLAR", "kimyasal-hortumlar"),
                ("KAUÇUK YANGIN HORTUMLARI", "kaucuk-yangin-hortumlari")
            }),
            ("PVC HORTUM", "pvc-hortum", new[]
            {
                ("TELLİ ŞEFFAF HORTUMLAR", "telli-seffaf-hortumlar"),
                ("SPİRAL EMİCİ VERİCİ HORTUMLAR", "spiral-emici-verici-hortumlar"),
                ("ÖRGÜLÜ SANAYİ, İNŞAAT VE TARIM HORTUMLARI", "orgulu-sanayi-insaat-ve-tarim-hortumlari"),
                ("YASSI VERİCİ SULAMA HORTUMLARI", "yassi-verici-sulama-hortumlari"),
                ("DÜZ ŞEFFAF HORTUMLAR", "duz-seffaf-hortumlar"),
                ("PVC YANGIN HORTUMLARI", "pvc-yangin-hortumlari")
            }),
            ("HİDROLİK HORTUM", "hidrolik-hortum", new[]
            {
                ("KAUÇUK HİDROLİK HORTUMLAR", "kaucuk-hidrolik-hortumlar"),
                ("TERMOPLASTİK HİDROLİK HORTUMLAR", "termoplastik-hidrolik-hortumlar"),
                ("SPİRAL KORUMALAR", "spiral-korumalar")
            }),
            ("PNÖMATİK HORTUM", "pnomatik-hortum", new[]
            {
                ("POLİÜRETAN (PU) HORTUMLAR", "poliuretan-pu-hortumlar"),
                ("POLİAMİD (PA) HORTUMLAR", "poliamid-pa-hortumlar"),
                ("POLİETİLEN (PE) HORTUMLAR", "polietilen-pe-hortumlar"),
                ("SİLİKON HORTUMLAR", "silikon-hortumlar")
            }),
            ("FLEKSIBLE TOZ - HAVA - DUMAN EMİŞ VE HAVALANDIRMA HORTUMU", "fleksible-toz-hava-duman-emis-ve-havalandirma-hortumu", new[]
            {
                ("HAVALANDIRMA VE DUMAN EMİŞ HORTUMLARI", "havalandirma-ve-duman-emis-hortumlari"),
                ("AĞAÇ SANAYİ VE TOZ EMİŞ HORTUMLARI", "agac-sanayi-ve-toz-emis-hortumlari")
            }),
            ("HORTUM BAĞLANTI ELEMANLARI", "hortum-baglanti-elemanlari", new[]
            {
                ("PASLANMAZ KAMLOKLAR", "paslanmaz-kamloklar"),
                ("POLİPROPLENE KAMLOKLAR", "poliproplene-kamloklar"),
                ("ALUMİNYUM KAMLOKLAR", "aluminyum-kamloklar"),
                ("TAKVİYELİ AĞIR İŞ HORTUM KELEPÇELERİ", "takviyeli-agir-is-hortum-kelepceleri"),
                ("AYARLI HORTUM KELEPÇELERİ", "ayarli-hortum-kelepceleri"),
                ("LOCK TİPİ KELEPÇELER", "lock-tipi-kelepceler"),
                ("PASLANMAZ FİTTİNGS", "paslanmaz-fittings")
            }),
            ("PVC ŞERİT PERDE VE LEVHA", "pvc-serit-perde-ve-levha", new[]
            {
                ("PVC ŞERİT PERDELER", "pvc-serit-perdeler"),
                ("PVC LEVHALAR", "pvc-levhalar")
            })
        };

        var official = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existing = await db.Categories.ToListAsync();

        for (var i = 0; i < tree.Length; i++)
        {
            var (name, slug, subs) = tree[i];
            official.Add(slug);
            var main = existing.FirstOrDefault(c => c.Slug == slug);
            if (main is null)
            {
                main = new Category
                {
                    Name = name,
                    Slug = slug,
                    Description = name,
                    SortOrder = i + 1,
                    IsActive = true,
                    IsMainCategory = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                db.Categories.Add(main);
                existing.Add(main);
                await db.SaveChangesAsync();
            }
            else
            {
                main.Name = name;
                main.Description = string.IsNullOrWhiteSpace(main.Description) ? name : main.Description;
                main.SortOrder = i + 1;
                main.IsActive = true;
                main.IsMainCategory = true;
                main.MainCategoryId = null;
            }

            for (var s = 0; s < subs.Length; s++)
            {
                var subDef = subs[s];
                official.Add(subDef.Slug);
                var sub = existing.FirstOrDefault(c => c.Slug == subDef.Slug);
                if (sub is null)
                {
                    sub = new Category
                    {
                        Name = subDef.Name,
                        Slug = subDef.Slug,
                        Description = subDef.Name,
                        SortOrder = s + 1,
                        IsActive = true,
                        IsMainCategory = false,
                        MainCategoryId = main.Id
                    };
                    db.Categories.Add(sub);
                    existing.Add(sub);
                }
                else
                {
                    sub.Name = subDef.Name;
                    sub.SortOrder = s + 1;
                    sub.IsActive = true;
                    sub.IsMainCategory = false;
                    sub.MainCategoryId = main.Id;
                }
            }
        }

        foreach (var leftover in existing.Where(c => !official.Contains(c.Slug)))
        {
            leftover.IsActive = false;
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureExtraProductsAsync(AppDbContext db)
    {
        var headers = await db.ProductFeaturesHeaders.ToListAsync();
        if (headers.Count == 0) return;

        var extras = new (string Sku, string Category, string Manufacturer, int Stock, int Sort, string Name, string ShortDesc, string Desc, decimal Price, (string Name, string Value)[] Features)[]
        {
            ("PVC-VE-001", "spiral-emici-verici-hortumlar", "SEL Hortum", 70, 7, "Mavi Verici Hortum",
                "Basınçlı su ve sıvı tahliyesi için spiral telli verici hortum.",
                "Sanayi ve tarımda basınçlı tahliye işleri için kullanılan mavi spiral PVC hortum.",
                148, new[] { ("Çap", "25-150"), ("Çalışma Basıncı", "6-8"), ("Çalışma Sıcaklığı", "-10 / +60"), ("Malzeme", "PVC"), ("Renk", "Mavi"), ("Menşe", "Türkiye") }),
            ("HID-YP-001", "kaucuk-hidrolik-hortumlar", "Continental AG", 45, 8, "Yüksek Basınç Hidrolik Hortum",
                "Ağır iş makineleri için 4 kat çelik sarımlı hidrolik hortum.",
                "Yüksek basınçlı mobil ve endüstriyel hidrolik sistemlerde kullanılan 4SH sınıfı hortum.",
                890, new[] { ("Çap", "10-51"), ("Çalışma Basıncı", "280-420"), ("Çalışma Sıcaklığı", "-40 / +120"), ("Malzeme", "Sentetik Kauçuk"), ("Renk", "Siyah"), ("Menşe", "Almanya") }),
            ("PVC-LV-001", "pvc-levhalar", "Extruflex", 150, 9, "Endüstriyel PVC Levha",
                "Tezgah, kaplama ve koruma için esnek PVC levha.",
                "Endüstriyel yüzey kaplama, darbe koruma ve hijyenik uygulama alanları için PVC levha.",
                210, new[] { ("Çap", "-"), ("Çalışma Basıncı", "-"), ("Çalışma Sıcaklığı", "-20 / +50"), ("Malzeme", "PVC"), ("Renk", "Şeffaf / Gri"), ("Menşe", "Fransa") }),
            ("KEP-001", "ayarli-hortum-kelepceleri", "Tecnica S.r.L", 400, 10, "Paslanmaz Hortum Kelepçesi",
                "Hortum bağlantılarında sızdırmazlık sağlayan paslanmaz kelepçe.",
                "W4 paslanmaz çelik hortum kelepçesi. Geniş çap aralığı ve yüksek tork dayanımı.",
                42, new[] { ("Çap", "12-160"), ("Çalışma Basıncı", "-"), ("Çalışma Sıcaklığı", "-"), ("Malzeme", "AISI 304"), ("Renk", "Gümüş"), ("Menşe", "İtalya") }),
            ("KOM-KM-001", "kompozit-kimyasal-hortumlar", "GASSO", 28, 11, "Kimyasal Kompozit Hortum",
                "Asit, solvent ve buhar transferi için PTFE iç katmanlı kompozit hortum.",
                "Kimyasal direnci yüksek, PTFE film katmanlı kompozit hortum. Tanker ve proses hatlarında kullanılır.",
                3120, new[] { ("Çap", "25-150"), ("Çalışma Basıncı", "10-14"), ("Çalışma Sıcaklığı", "-30 / +150"), ("Malzeme", "PTFE / PVC"), ("Renk", "Beyaz / Yeşil"), ("Menşe", "İspanya") })
        };

        var existing = (await db.Products.Select(p => p.Sku).ToListAsync()).ToHashSet();
        var added = false;
        foreach (var extra in extras)
        {
            if (existing.Contains(extra.Sku)) continue;
            var (product, features) = CreateProduct(
                extra.Sku, extra.Category, extra.Manufacturer, extra.Stock, extra.Sort,
                extra.Name, extra.ShortDesc, extra.Desc, extra.Price, null, extra.Features);
            foreach (var (name, value) in features)
            {
                var header = headers.FirstOrDefault(h => h.Name == name);
                if (header is null) continue;
                product.Features.Add(new ProductFeaturesDetail { Header = header, Value = value });
            }
            db.Products.Add(product);
            added = true;
        }

        if (added)
        {
            await db.SaveChangesAsync();
        }
    }

    private static async Task BindProductsToSubcategoriesAsync(AppDbContext db)
    {
        var map = new Dictionary<string, string>
        {
            ["PVC-CT-001"] = "telli-seffaf-hortumlar",
            ["PVC-YE-001"] = "spiral-emici-verici-hortumlar",
            ["PVC-VE-001"] = "spiral-emici-verici-hortumlar",
            ["HID-001"] = "kaucuk-hidrolik-hortumlar",
            ["HID-YP-001"] = "kaucuk-hidrolik-hortumlar",
            ["PVC-SP-001"] = "pvc-serit-perdeler",
            ["PVC-LV-001"] = "pvc-levhalar",
            ["KAM-AL-001"] = "aluminyum-kamloklar",
            ["KEP-001"] = "ayarli-hortum-kelepceleri",
            ["KOM-001"] = "kompozit-akaryakit-hortumlari",
            ["KOM-KM-001"] = "kompozit-kimyasal-hortumlar"
        };

        var categories = await db.Categories.ToListAsync();
        var products = await db.Products.Include(p => p.ProductCategories).ToListAsync();
        var changed = false;

        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.Manufacturer))
            {
                product.Manufacturer = product.Sku switch
                {
                    "PVC-CT-001" or "PVC-YE-001" or "PVC-VE-001" => "SEL Hortum",
                    "KOM-001" or "KOM-KM-001" => "GASSO",
                    "HID-001" or "HID-YP-001" => "Continental AG",
                    "PVC-SP-001" or "PVC-LV-001" => "Extruflex",
                    "KAM-AL-001" or "KEP-001" => "Tecnica S.r.L",
                    _ => product.Manufacturer
                };
                changed = true;
            }

            if (!map.TryGetValue(product.Sku, out var subSlug))
            {
                if (product.ProductCategories.Count == 0 && !string.IsNullOrWhiteSpace(product.Category))
                {
                    var fallback = categories.FirstOrDefault(c => c.Slug == product.Category);
                    if (fallback is not null)
                    {
                        db.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = fallback.Id });
                        changed = true;
                    }
                }
                continue;
            }

            var sub = categories.FirstOrDefault(c => c.Slug == subSlug);
            if (sub is null) continue;

            if (product.Category != sub.Slug)
            {
                product.Category = sub.Slug;
                changed = true;
            }

            var already = product.ProductCategories.Any(pc => pc.CategoryId == sub.Id) && product.ProductCategories.Count == 1;
            if (already) continue;

            db.ProductCategories.RemoveRange(product.ProductCategories);
            db.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = sub.Id });
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
    }

    private static List<ProductFeaturesHeader> DefaultFeatureHeaders() =>
    [
        new() { Name = "Çalışma Sıcaklığı", Unit = "°C", SortOrder = 1 },
        new() { Name = "Çalışma Basıncı", Unit = "bar", SortOrder = 2 },
        new() { Name = "Çap", Unit = "mm", SortOrder = 3 },
        new() { Name = "Uzunluk", SortOrder = 4 },
        new() { Name = "Menşe", SortOrder = 5 },
        new() { Name = "Kullanım Alanları", SortOrder = 6 },
        new() { Name = "Renk", SortOrder = 7 },
        new() { Name = "Malzeme", SortOrder = 8 }
    ];

    private static async Task EnsureFeatureHeadersAsync(AppDbContext db)
    {
        var existing = await db.ProductFeaturesHeaders.ToListAsync();
        var changed = false;
        foreach (var def in DefaultFeatureHeaders())
        {
            var header = existing.FirstOrDefault(h => string.Equals(h.Name, def.Name, StringComparison.OrdinalIgnoreCase));
            if (header is null)
            {
                db.ProductFeaturesHeaders.Add(def);
                existing.Add(def);
                changed = true;
            }
            else if (header.SortOrder != def.SortOrder || header.Unit != def.Unit)
            {
                header.SortOrder = def.SortOrder;
                header.Unit = def.Unit;
                changed = true;
            }
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
    }

    private static string? TryFindCatalogPath()
    {
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seed", "teknofleks-products.json"),
            Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "teknofleks-products.json"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data", "Seed", "teknofleks-products.json"))
        };
        return candidates.FirstOrDefault(File.Exists);
    }

    private static async Task ImportTeknofleksProductsAsync(AppDbContext db, string catalogPath)
    {
        var json = await File.ReadAllTextAsync(catalogPath);
        var catalog = JsonSerializer.Deserialize<List<CatalogProduct>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? [];
        if (catalog.Count == 0) return;

        var existingCount = await db.Products.CountAsync();
        if (existingCount >= catalog.Count)
        {
            return;
        }

        var headers = await db.ProductFeaturesHeaders.ToListAsync();
        var categories = await db.Categories.ToListAsync();
        var products = await db.Products
            .Include(p => p.Translations)
            .Include(p => p.Features)
            .Include(p => p.Images)
            .Include(p => p.ProductCategories)
            .Include(p => p.Prices)
            .ToListAsync();
        var bySku = products.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);
        var catalogSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in catalog)
        {
            if (string.IsNullOrWhiteSpace(item.Sku) || string.IsNullOrWhiteSpace(item.Name)) continue;
            catalogSkus.Add(item.Sku);

            var categorySlug = item.CategorySlug ?? string.Empty;
            var category = categories.FirstOrDefault(c => c.Slug == categorySlug);
            if (!bySku.TryGetValue(item.Sku, out var product))
            {
                var created = CreateProduct(
                    item.Sku,
                    categorySlug,
                    string.Empty,
                    100,
                    item.SortOrder,
                    item.Name,
                    item.ShortDescription ?? item.Description ?? string.Empty,
                    item.Description ?? item.ShortDescription ?? string.Empty,
                    0,
                    null);
                product = created.Product;
                if (!string.IsNullOrWhiteSpace(item.Slug))
                {
                    foreach (var tr in product.Translations)
                    {
                        tr.Slug = tr.LanguageCode == "en" ? item.Slug + "-en" : item.Slug;
                    }
                }
                db.Products.Add(product);
                products.Add(product);
                bySku[item.Sku] = product;
            }
            else
            {
                product.IsActive = true;
                product.SortOrder = item.SortOrder;
                product.Category = categorySlug;
                product.Stock = product.Stock < 1 ? 100 : product.Stock;
                product.UpdatedAt = DateTimeOffset.UtcNow;

                UpsertTranslation(product, "tr", item.Name, item.ShortDescription, item.Description, item.Slug);
                UpsertTranslation(product, "en", item.Name, item.ShortDescription, item.Description,
                    string.IsNullOrWhiteSpace(item.Slug) ? null : item.Slug + "-en");
            }

            if (category is not null)
            {
                var already = product.ProductCategories.Any(pc => pc.CategoryId == category.Id || pc.Category?.Id == category.Id)
                    && product.ProductCategories.Count == 1;
                if (!already)
                {
                    if (product.Id > 0 && product.ProductCategories.Count > 0)
                    {
                        db.ProductCategories.RemoveRange(product.ProductCategories);
                    }
                    product.ProductCategories.Clear();
                    var link = new ProductCategory { CategoryId = category.Id, Category = category };
                    if (product.Id > 0)
                    {
                        link.ProductId = product.Id;
                    }
                    product.ProductCategories.Add(link);
                }
            }

            foreach (var feature in item.Features ?? [])
            {
                var headerName = NormalizeFeatureName(feature.Name);
                if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(feature.Value)) continue;
                var header = headers.FirstOrDefault(h => string.Equals(h.Name, headerName, StringComparison.OrdinalIgnoreCase));
                if (header is null) continue;
                var value = feature.Value.Length > 300 ? feature.Value[..300] : feature.Value;
                var detail = product.Features.FirstOrDefault(f => f.Header == header || f.ProductFeaturesHeaderId == header.Id);
                if (detail is null)
                {
                    product.Features.Add(new ProductFeaturesDetail { Header = header, Value = value });
                }
                else if (detail.Value != value)
                {
                    detail.Value = value;
                }
            }

            if (!string.IsNullOrWhiteSpace(item.ImagePath) && !product.Images.Any())
            {
                product.Images.Add(new ProductImage
                {
                    FilePath = item.ImagePath,
                    AltText = item.Name,
                    IsPrimary = true,
                    SortOrder = 0
                });
            }
        }

        foreach (var leftover in products.Where(p => !catalogSkus.Contains(p.Sku) && p.IsActive))
        {
            leftover.IsActive = false;
            leftover.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private static void UpsertTranslation(Product product, string lang, string name, string? shortDesc, string? desc, string? slug)
    {
        var tr = product.Translations.FirstOrDefault(t => t.LanguageCode == lang);
        var safeShort = string.IsNullOrWhiteSpace(shortDesc) ? desc : shortDesc;
        if (safeShort?.Length > 500) safeShort = safeShort[..497].Trim() + "...";
        if (tr is null)
        {
            product.Translations.Add(new ProductTranslation
            {
                LanguageCode = lang,
                Name = name,
                ShortDescription = safeShort,
                Description = desc,
                Slug = slug ?? Slugify(name) + (lang == "en" ? "-en" : string.Empty)
            });
            return;
        }

        tr.Name = name;
        tr.ShortDescription = safeShort;
        tr.Description = desc;
        if (!string.IsNullOrWhiteSpace(slug)) tr.Slug = slug;
    }

    private static string NormalizeFeatureName(string? name)
    {
        var key = (name ?? string.Empty).Trim().TrimEnd(':').Trim();
        return key.ToLowerInvariant() switch
        {
            "çap" or "cap" => "Çap",
            "çalışma basıncı" or "calisma basinci" => "Çalışma Basıncı",
            "çalışma sıcaklığı" or "calisma sicakligi" => "Çalışma Sıcaklığı",
            "uzunluk" => "Uzunluk",
            "menşe" or "menşei" or "mense" => "Menşe",
            "kullanım alanları" or "kullanim alanlari" => "Kullanım Alanları",
            "renk" or "renkler" => "Renk",
            "malzeme" => "Malzeme",
            _ => key
        };
    }

    private sealed class CatalogProduct
    {
        public string Sku { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? CategorySlug { get; set; }
        public string? ImagePath { get; set; }
        public int SortOrder { get; set; }
        public List<CatalogFeature> Features { get; set; } = [];
    }

    private sealed class CatalogFeature
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    private static (Product Product, (string Name, string Value)[] Features) CreateProduct(
        string sku, string category, string manufacturer, int stock, int sort, string name, string shortDesc, string desc, decimal price, Campaign? campaign,
        params (string Name, string Value)[] features)
    {
        var product = new Product
        {
            Sku = sku,
            Category = category,
            Manufacturer = manufacturer,
            Stock = stock,
            IsActive = true,
            SortOrder = sort,
            CreatedAt = DateTimeOffset.UtcNow
        };

        product.Translations.Add(new ProductTranslation
        {
            LanguageCode = "tr",
            Name = name,
            ShortDescription = shortDesc,
            Description = desc,
            Slug = Slugify(name)
        });

        product.Translations.Add(new ProductTranslation
        {
            LanguageCode = "en",
            Name = name,
            ShortDescription = shortDesc,
            Description = desc,
            Slug = Slugify(name) + "-en"
        });

        product.Prices.Add(new ProductPrice
        {
            Amount = price,
            Currency = "TRY",
            Campaign = campaign,
            IsCurrent = true,
            ValidFrom = DateTimeOffset.UtcNow
        });

        return (product, features);
    }

    private static string Slugify(string value)
    {
        var map = new Dictionary<char, string>
        {
            ['ç'] = "c", ['ğ'] = "g", ['ı'] = "i", ['ö'] = "o", ['ş'] = "s", ['ü'] = "u",
            ['Ç'] = "c", ['Ğ'] = "g", ['İ'] = "i", ['Ö'] = "o", ['Ş'] = "s", ['Ü'] = "u"
        };
        var sb = new StringBuilder();
        foreach (var c in value.Trim().ToLowerInvariant())
        {
            if (map.TryGetValue(c, out var r)) sb.Append(r);
            else if (char.IsLetterOrDigit(c)) sb.Append(c);
            else if (c is ' ' or '-' or '_') sb.Append('-');
        }
        return Regex.Replace(sb.ToString(), "-+", "-").Trim('-');
    }
}
