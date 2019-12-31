using System.ComponentModel.DataAnnotations;

namespace Sanmei_AirConditioner.Configuration.Dto
{
    public class ChangeUiThemeInput
    {
        [Required]
        [MaxLength(32)]
        public string Theme { get; set; }
    }
}