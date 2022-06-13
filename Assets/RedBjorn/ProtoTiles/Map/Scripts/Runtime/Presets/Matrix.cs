using RedBjorn.Utils;

namespace RedBjorn.ProtoTiles
{
    public class Matrix
    {
        public class Cube
        {
            public class Fractional
            {
                public const float x11 = 2f / 3f;
                public const float x12 = 0f;
                public const float x21 = -1f / 3f;
                public const float x22 = Constants.Sqrt3 / 3f;
            }

            public class HexToWorld
            {
                public const float x11 = 1.5f;
                public const float x12 = 0f;
                public const float x21 = Constants.Cos30;
                public const float x22 = Constants.Sqrt3;
            }
        }
    }
}
