using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared;

namespace Shared.VersionTrackerEntities;

[Table("version_trackers")]
public class VersionTracker : ITrackable
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("table_name")]
    public string TableName { get; set; }

    [Column("last_version_timestamp")]
    public DateTime LastVersionTimestamp { get; set; }

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("updated_on")]
    public DateTime UpdatedOn { get; set; }
}
