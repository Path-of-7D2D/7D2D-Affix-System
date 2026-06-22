namespace AffixSystem.Affixes
{
    internal static class AffixEligibility
    {
        private static readonly FastTags<TagGroup.Global> WeaponTags = FastTags<TagGroup.Global>.Parse("weapon");
        private static readonly FastTags<TagGroup.Global> ToolTags = FastTags<TagGroup.Global>.Parse("tool");
        private static readonly FastTags<TagGroup.Global> ArmorTags = FastTags<TagGroup.Global>.Parse("armor");

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
                (itemClass.HasAnyTags(WeaponTags) ||
                    itemClass.HasAnyTags(ToolTags) ||
                    itemClass.HasAnyTags(ArmorTags));
        }

        public static bool TryGetDisplayableState(ItemValue itemValue, out AffixItemState state)
        {
            return AffixItemState.TryReadDisplayable(itemValue, out state);
        }
    }
}
