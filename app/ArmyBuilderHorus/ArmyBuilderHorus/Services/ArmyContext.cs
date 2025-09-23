namespace ArmyBuilderHorus.Services;

/// <summary>
/// Contexte d'armée passé au RulesEngine : armée, allégeance, légion, rite, etc.
/// Tout est optionnel sauf ArmyId. Tu peux compléter au fur et à mesure.
/// </summary>
public sealed class ArmyContext
{
    /// <summary>Armée principale (ex: "LEGIONES_ASTARTES")</summary>
    public string ArmyId { get; init; } = "LEGIONES_ASTARTES";

    /// <summary>LOYALIST ou TRAITOR (optionnel à ce stade)</summary>
    public string Allegiance { get; init; } = "LOYALIST";

    /// <summary>Légion si applicable (ex: "DARK_ANGELS")</summary>
    public string? LegionId { get; init; }

    /// <summary>Rite de guerre sélectionné (ex: "PRIDE_OF_THE_LEGION")</summary>
    public string? RiteId { get; init; }

    /// <summary>Est-ce un détachement allié ? (pour plus tard)</summary>
    public bool IsAlliedDetachment { get; init; } = false;

    /// <summary>Référence vers l'armée/légion primaire si ceci est un allié (pour plus tard)</summary>
    public string? PrimaryArmyId { get; init; }
    public string? PrimaryLegionId { get; init; }

    // Petites fabriques pratiques si tu veux construire rapidement le contexte :
    public static ArmyContext For(string armyId, string? riteId = null, string allegiance = "LOYALIST", string? legionId = null)
        => new() { ArmyId = armyId, RiteId = riteId, Allegiance = allegiance, LegionId = legionId };
}
