using System.Text.Json.Serialization;

namespace ArmyBuilderHorus.Models;

public sealed class Catalog
{
    public string version { get; set; } = "";
    public List<Army> armies { get; set; } = new();
    public List<Org> orgs { get; set; } = new();
    public List<Rite> rites { get; set; } = new();

    public List<ArmyUnit> units { get; set; } = new();
}

public sealed class Army
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public List<AvailableFoc> available_focs { get; set; } = new();     // [{ id, label }]
    public Dictionary<string, List<string>> available_rites { get; set; } = new(); // FOC_ID -> [RITE_ID...]
}

public sealed class ArmyUnit
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public string faction { get; set; } = "";
    public string slot { get; set; } = "";  // HQ/TROOPS/...
    public int base_cost { get; set; }
    public UnitSize? size { get; set; }
    public List<OptionGroup> options { get; set; } = new ();
}

public sealed class UnitSize
{
    public int min { get; set; }          // ex: 10
    public int max { get; set; }          // ex: 20
    public int step { get; set; } = 1;    // ex: 1 ou 5
    public int base_models { get; set; }  // ex: 10
    public int base_points { get; set; }  // ex: 100
    public int extra_model_points { get; set; } // ex: 10 (par figurine au-dessus de base_models)
}
public sealed class OptionGroup
{
    public string id { get; set; } = "";
    public string label { get; set; } = "";
    public string type { get; set; } = "choice";      // "choice" | "counted"
    public int? max { get; set; }
    public LimitFormula? limit_formula { get; set; }
    public List<OptionChoice> choices { get; set; } = new();

    // ✨ nouveau
    public ConditionBlock? available_when { get; set; } = null;
    public List<string>? excludes_groups { get; set; } = null;   // groupes mutuellement exclus
    public List<string>? replaces { get; set; } = null;          // équipements/slots remplacés
    public string? applies_to_model_set { get; set; } = null;    // "ALL", "ALL_BUT_SERGEANT"...
}

public sealed class OptionChoice
{
    public string id { get; set; } = "";
    public string label { get; set; } = "";
    public int points { get; set; }

    // ✨ nouveau
    public ConditionBlock? requires { get; set; } = null;
    public bool unique_in_army { get; set; } = false;
    public List<string>? grants_tags { get; set; } = null;      // coût de cette option
}

// Autorise des limites “par tranche” : ex. step=10, per_step=2 => floor(size/10)*2
public sealed class LimitFormula
{
    public int step { get; set; }      // taille palier (ex: 10)
    public int per_step { get; set; }  // droits par palier (ex: 2)
}
public sealed class AvailableFoc { public string id { get; set; } = ""; public string label { get; set; } = ""; }

public sealed class Org
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public Dictionary<string, Slot> slots { get; set; } = new(); // "HQ": {min,max}...
}
public sealed class Slot { public int min { get; set; } public int max { get; set; } }

public sealed class Rite
{
    public string id { get; set; } = "";
    public string applies_to { get; set; } = ""; // army id
    public RiteRequires? requires { get; set; }
    public List<string> messages { get; set; } = new();
}
public sealed class RiteRequires { public int? min_points_gte { get; set; } }
