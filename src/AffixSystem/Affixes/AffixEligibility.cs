namespace AffixSystem.Affixes
{
    internal static class AffixEligibility
    {
        private static readonly FastTags<TagGroup.Global> WeaponTags = FastTags<TagGroup.Global>.Parse("weapon");
        private static readonly FastTags<TagGroup.Global> ToolTags = FastTags<TagGroup.Global>.Parse("tool");

        public static bool IsSupportedBaseItem(ItemValue itemValue)
        {
            return itemValue != null &&
                !itemValue.IsEmpty() &&
                itemValue.HasQuality &&
                IsSupportedBaseItem(itemValue.ItemClass);
        }

        public static bool IsSupportedBaseItem(ItemClass itemClass)
        {
            return itemClass != null &&
                (itemClass.HasAnyTags(WeaponTags) || itemClass.HasAnyTags(ToolTags));
        }

        public static bool TryGetDisplayableState(ItemValue itemValue, out AffixItemState state)
        {
            return AffixItemState.TryReadDisplayable(itemValue, out state);
        }
    }
}
