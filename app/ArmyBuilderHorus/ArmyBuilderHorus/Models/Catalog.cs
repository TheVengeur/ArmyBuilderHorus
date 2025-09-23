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

public sealed class AvailableFoc { public string id { get; set; } = ""; public string label { get; set; } = ""; }

public sealed class ArmyUnit
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public string faction { get; set; } = "";
    public string slot { get; set; } = "";  // HQ/TROOPS/...
    public int base_cost { get; set; }

    // Escouade (optionnelle)
    public UnitSize? size { get; set; }

    // Options V2
    public List<OptionGroup> options { get; set; } = new();

    // ——— Fiche d’unité (facultatif / pour l’affichage) ———
    public List<UnitProfile>? profiles { get; set; }
    public List<CompositionEntry>? composition { get; set; }
    public Dictionary<string, List<string>>? unit_types { get; set; }
    public List<string>? wargear_base { get; set; }
    public List<string>? special_rules { get; set; }
    public List<DedicatedTransport>? dedicated_transport { get; set; }

    // Traits & contraintes
    public List<string> traits { get; set; } = new();
    public bool? compulsory_eligible { get; set; }   // null => true
}

public sealed class UnitProfile
{
    public string name { get; set; } = "";
    public Dictionary<string, string> statline { get; set; } = new();
}

public sealed class CompositionEntry
{
    public string model { get; set; } = "";
    public int count { get; set; }
}

public sealed class DedicatedTransport
{
    public string name { get; set; } = "";
    public TransportLimit? limit { get; set; }
    public string? notes { get; set; }
}
public sealed class TransportLimit { public int? max_models { get; set; } }

// Taille d’escouade
public sealed class UnitSize
{
    public int min { get; set; }          // ex: 10
    public int max { get; set; }          // ex: 20
    public int step { get; set; } = 1;    // ex: 1 ou 5
    public int base_models { get; set; }  // ex: 10
    public int base_points { get; set; }  // ex: 100
    public int extra_model_points { get; set; } // ex: 10 (par figurine au-dessus de base_models)
}

// Groupe d’options
public sealed class OptionGroup
{
    public string id { get; set; } = "";
    public string label { get; set; } = "";
    public string type { get; set; } = "choice";      // "choice" | "counted" (UI supportée)
    public int? max { get; set; }                     // max total si "counted"
    public LimitFormula? limit_formula { get; set; }  // ex: {step:10, per_step:2}
    public List<OptionChoice> choices { get; set; } = new();

    // Règles avancées (facultatives)
    public ConditionBlock? available_when { get; set; } = null;
    public List<string>? excludes_groups { get; set; } = null;   // groupes mutuellement exclus
    public List<string>? replaces { get; set; } = null;          // équipements/slots remplacés (pour affichage ultérieur)
    public string? applies_to_model_set { get; set; } = null;    // "ALL", "ALL_BUT_SERGEANT", etc. (affichage futur)
}

public sealed class OptionChoice
{
    public string id { get; set; } = "";
    public string label { get; set; } = "";
    public int points { get; set; }
    public ConditionBlock? requires { get; set; } = null;
    public bool unique_in_army { get; set; } = false;
    public List<string>? grants_tags { get; set; } = null;
}

// Autorise des limites “par tranche” : ex. step=10, per_step=2 => floor(size/10)*2
public sealed class LimitFormula { public int step { get; set; } public int per_step { get; set; } }

// FOC
public sealed class Org
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public Dictionary<string, Slot> slots { get; set; } = new(); // "HQ": {min,max}...
}
public sealed class Slot { public int min { get; set; } public int max { get; set; } }

// Rites
public sealed class Rite
{
    public string id { get; set; } = "";
    public string applies_to { get; set; } = ""; // army id
    public RiteRequires? requires { get; set; }
    public List<string> messages { get; set; } = new();
    public bool allow_support_troops_compulsory { get; set; } = false; // NEW
}
public sealed class RiteRequires { public int? min_points_gte { get; set; } }
