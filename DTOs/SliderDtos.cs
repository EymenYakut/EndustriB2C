using System.ComponentModel.DataAnnotations;
using EndustriB2C.Entities;

namespace EndustriB2C.DTOs;

public class SliderDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class SliderRequest
{
    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? Subtitle { get; set; }

    [MaxLength(80)]
    public string? ButtonText { get; set; }

    [MaxLength(300)]
    public string? ButtonUrl { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public static class SliderMaps
{
    public static SliderDto ToDto(this HomeSlider s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Subtitle = s.Subtitle,
        ButtonText = s.ButtonText,
        ButtonUrl = s.ButtonUrl,
        ImageUrl = s.ImageUrl,
        SortOrder = s.SortOrder,
        IsActive = s.IsActive
    };
}
