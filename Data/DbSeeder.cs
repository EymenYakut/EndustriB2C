using System.Text;
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

        var headers = new List<ProductFeaturesHeader>
        {
            new() { Name = "Çap", Unit = "mm", SortOrder = 1 },
            new() { Name = "Çalışma Basıncı", Unit = "bar", SortOrder = 2 },
            new() { Name = "Çalışma Sıcaklığı", Unit = "°C", SortOrder = 3 },
            new() { Name = "Malzeme", SortOrder = 4 },
            new() { Name = "Renk", SortOrder = 5 },
            new() { Name = "Menşe", SortOrder = 6 }
        };
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
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "PVC ve Kauçuk Endüstriyel Hortumlar", Slug = "pvc-kauçuk", Description = "Şeffaf, emici, verici ve özel amaçlı hortumlar", SortOrder = 1, IsActive = true },
                new Category { Name = "Hidrolik Hortumlar", Slug = "hidrolik", Description = "Yüksek basınçlı hidrolik sistem çözümleri", SortOrder = 2, IsActive = true },
                new Category { Name = "PVC Şerit ve Levhalar", Slug = "pvc-serit", Description = "Endüstriyel şerit perde ve levha sistemleri", SortOrder = 3, IsActive = true },
                new Category { Name = "Kamloklar ve Kelepçeler", Slug = "kamlok", Description = "Hızlı bağlantı elemanları ve fittings", SortOrder = 4, IsActive = true },
                new Category { Name = "Kompozit Hortumlar", Slug = "kompozit", Description = "Akaryakıt, kimyasal, LPG ve buhar transferi", SortOrder = 5, IsActive = true }
            );
            await db.SaveChangesAsync();
        }

        var categories = await db.Categories.ToListAsync();
        var products = await db.Products.Include(p => p.ProductCategories).ToListAsync();
        var changed = false;
        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.Manufacturer))
            {
                product.Manufacturer = product.Sku switch
                {
                    "PVC-CT-001" or "PVC-YE-001" => "SEL Hortum",
                    "KOM-001" => "GASSO",
                    "HID-001" => "Continental AG",
                    "PVC-SP-001" => "Extruflex",
                    "KAM-AL-001" => "Tecnica S.r.L",
                    _ => product.Manufacturer
                };
                changed = true;
            }

            if (product.ProductCategories.Count > 0 || string.IsNullOrWhiteSpace(product.Category))
            {
                continue;
            }

            var match = categories.FirstOrDefault(c => c.Slug == product.Category);
            if (match is null) continue;
            db.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = match.Id });
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync();
        }
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
