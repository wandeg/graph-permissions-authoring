using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthoringPrototype;

public class AuthoringInput
{

    [JsonPropertyName("$schema")]
    public string Schema { get; set; }

    public PathContainer Paths { get; set; }
}
