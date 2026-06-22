namespace AffixSystem.Affixes
{
    internal sealed class AffixInstance
    {
        public AffixInstance(string definitionId, int tier, int statValue)
        {
            DefinitionId = definitionId;
            Tier = tier;
            StatValue = statValue;
        }

        public string DefinitionId { get; }

        public int Tier { get; }

        public int StatValue { get; }
    }
}

