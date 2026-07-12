using System.ComponentModel.DataAnnotations.Schema;

namespace Shared;

public interface ITrackable
{
    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("updated_on")]
    public DateTime UpdatedOn { get; set; }
}
