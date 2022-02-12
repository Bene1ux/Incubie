using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Incubie
{
    public class MinimapTextInfo
    {
        public MinimapTextInfo() { }

        public MinimapTextInfo(string text, int fontSize, Color fontColor, Color fontBackgroundColor, int textWrapLength, int textOffsetY)
        {
            Text = text;
            FontSize = fontSize;
            FontColor = fontColor;
            FontBackgroundColor = fontBackgroundColor;
            TextWrapLength = textWrapLength;
            TextOffsetY = textOffsetY;
        }

        public string Text { get; set; }
        public int FontSize { get; set; }
        public Color FontColor { get; set; }
        public Color FontBackgroundColor { get; set; }
        public int TextWrapLength { get; set; }
        public int TextOffsetY { get; set; }
    }
}
