using Microsoft.Xna.Framework;
using GUI;
using Xunit;

namespace GUI.Tests
{
    public class ControlTests
    {
        [Fact]
        public void Tag_PropertySetAndGet()
        {
            var btn = new Button(new Rectangle(0,0,10,10), "T");
            btn.Tag = "meta";
            Assert.Equal("meta", btn.Tag);
        }
    }
}