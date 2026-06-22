namespace AffixSystem.Affixes
{
    internal static class AffixEligibility
    {
        private static readonly FastTags<TagGroup.Global> WeaponTags = FastTags<TagGroup.Global>.Parse("weapon");

        public static bool IsSupportedBaseItem(ItemValue itemValue)
        {
            return itemValue != null && !itemValue.IsEmpty() && IsSupportedBaseItem(itemValue.ItemClass);
        }

        public static bool IsSupportedBaseItem(ItemClass itemClass)
        {
            return itemClass != null && itemClass.HasAnyTags(WeaponTags);
        }

        public static bool TryGetDisplayableState(ItemValue itemValue, out AffixItemState state)
        {
            return AffixItemState.TryReadDisplayable(itemValue, out state);
        }
    }
}
