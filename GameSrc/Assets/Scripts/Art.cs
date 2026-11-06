using UnityEngine;

namespace MotMam
{
    public static class Art
    {
        public static readonly Color Ink = new(0.173f, 0.141f, 0.125f);
        public static readonly Color Son = new(0.549f, 0.184f, 0.224f);
        public static readonly Color Leaf = new(0.184f, 0.420f, 0.310f);
        public static readonly Color Cham = new(0.122f, 0.306f, 0.475f);
        public static readonly Color Reed = new(0.243f, 0.420f, 0.400f);
        public static readonly Color Flame = new(0.886f, 0.290f, 0.165f);
        public static readonly Color Paper = new(0.965f, 0.941f, 0.902f);

        public static Color Accent(GuestId guest)
        {
            if (guest == GuestId.Ho) return Leaf;
            if (guest == GuestId.Me) return Cham;
            if (guest == GuestId.Co) return Reed;
            if (guest == GuestId.Dokkaebi) return Flame;
            return Son;
        }

        public static string GuestLabel(GuestMode mode)
        {
            switch (mode)
            {
                case GuestMode.Ho: return "Hổ";
                case GuestMode.Me: return "Mê";
                case GuestMode.GumihoHo: return "Gumiho · Hổ";
                case GuestMode.Co: return "Cò";
                case GuestMode.Dokkaebi: return "Dokkaebi";
                default: return "Gumiho";
            }
        }

        public static string GuestShort(GuestMode mode)
        {
            if (mode == GuestMode.GumihoHo) return "Gumi+Hổ";
            return GuestLabel(mode);
        }

        public static GuestId GuestOf(GuestMode mode)
        {
            switch (mode)
            {
                case GuestMode.Ho: return GuestId.Ho;
                case GuestMode.Me: return GuestId.Me;
                case GuestMode.Co: return GuestId.Co;
                case GuestMode.Dokkaebi: return GuestId.Dokkaebi;
                default: return GuestId.Gumiho;
            }
        }

        public static string DishLabel(string dishId)
        {
            switch (dishId)
            {
                case "banh_mi": return "Bánh mì";
                case "goi_cuon": return "Gỏi cuốn";
                case "pho": return "Phở";
                case "ca_phe": return "Cà phê sữa đá";
                case "gimbap": return "Gimbap";
                case "songpyeon": return "Songpyeon";
                case "tteokbokki": return "Tteokbokki";
                case "bibimbap": return "Bibimbap";
                default: return dishId;
            }
        }
    }
}
