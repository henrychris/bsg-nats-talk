using System.ComponentModel.DataAnnotations;

namespace FinalNatsDemo.Common.Settings
{
    public class DatabaseSettings
    {
        [Required(AllowEmptyStrings = false)]
        public string ConnectionString { get; set; } = null!;
    }
}
