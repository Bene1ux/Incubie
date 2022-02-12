using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Incubie
{
    public class StoredEntity
    {
        public float EntityZ;
        public Vector2 GridPos;
        public long LongID;
        public MinimapTextInfo TextureInfo;

        public StoredEntity(float entityZ, Vector2 gridPos, long longID, MinimapTextInfo textureInfo)
        {
            EntityZ = entityZ;
            GridPos = gridPos;
            LongID = longID;
            TextureInfo = textureInfo;
        }
    }
}
