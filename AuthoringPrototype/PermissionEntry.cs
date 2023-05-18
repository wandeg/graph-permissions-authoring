using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthoringPrototype;

public class PermissionEntry
{
    // TODO: Update input to have 'methods'
    [JsonPropertyName("method")]
    public List<string> Methods { get; set; }

    // TODO: Update input to have 'permissions'
    [JsonPropertyName("permission")]
    public List<string> Permissions { get; set; }

    // TODO: Update input to have 'schemes'
    [JsonPropertyName("schema")]
    public List<string> Schemes { get; set; }
}
