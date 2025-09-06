using Microsoft.Xna.Framework;
using GUI;
using Xunit;
using Microsoft.Xna.Framework.Input;

namespace GUI.Tests
{
    public class ButtonTests
    {
        [Fact]
        public void Clicked_EventRaised_WhenClickedInsideBounds()
        {
            var button = new Button(new Rectangle(0,0,10,10), "Test");
            bool clicked = false;
            button.Clicked += (_, __) => clicked = true;
            var input = new InputState();
            input.Update();
            // simulate click at (5,5)
            input.Previous = new MouseState(5,5,0, ButtonState.Released,0,0,0,0);
            input.Current = new MouseState(5,5,0, ButtonState.Pressed,0,0,0,0);
            button.Update(input);
            Assert.True(clicked);
        }

        [Fact]
        public void Clicked_EventNotRaised_WhenOutsideBounds()
        {
            var button = new Button(new Rectangle(0,0,10,10), "Test");
            bool clicked = false;
            button.Clicked += (_, __) => clicked = true;
            var input = new InputState();
            input.Update();
            input.Previous = new MouseState(20,20,0, ButtonState.Released,0,0,0,0);
            input.Current = new MouseState(20,20,0, ButtonState.Pressed,0,0,0,0);
            button.Update(input);
            Assert.False(clicked);
        }
    }
}