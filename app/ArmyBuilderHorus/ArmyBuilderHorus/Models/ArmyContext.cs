namespace ArmyBuilderHorus.Models;

public sealed class ArmyContext
{
    // Contexte de CE détachement
    public string ArmyId { get; set; } = "LEGIONES_ASTARTES";   // ex: LEGIONES_ASTARTES, SOLAR_AUXILIA
    public string Allegiance { get; set; } = "LOYALIST";        // LOYALIST | TRAITOR
    public string? LegionId { get; set; } = null;               // ex: DARK_ANGELS (pour LA uniquement)
    public string? RiteId { get; set; } = null;
    public bool IsAlliedDetachment { get; set; } = false;       // ce détachement est-il un allié ?

    // Infos sur le primaire (utiles pour l’allié)
    public string? PrimaryArmyId { get; set; } = null;          // ex: LEGIONES_ASTARTES
    public string? PrimaryLegionId { get; set; } = null;        // si le primaire est une Légion
}
