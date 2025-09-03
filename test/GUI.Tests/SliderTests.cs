using Microsoft.Xna.Framework;
using GUI;
using GUI.Events;
using Microsoft.Xna.Framework.Input;
using Xunit;

namespace GUI.Tests
{
    public class SliderTests
    {
        [Fact]
        public void ValueChanged_EventRaised_WhenDraggedInsideBounds()
        {
            var slider = new Slider(new Rectangle(0, 0, 100, 10), 0f, 1f, 0f, "Test");
            float? newValue = null;
            slider.ValueChanged += (_, e) => newValue = e.NewValue;
            var input = new InputState
            {
                Previous = new MouseState(0, 0, 0, ButtonState.Released, 0, 0, 0, 0),
                Current = new MouseState(50, 5, 0, ButtonState.Pressed, 0, 0, 0, 0)
            };
            slider.Update(input);
            Assert.NotNull(newValue);
            Assert.InRange(newValue.Value, 0.49f, 0.51f);
        }

        [Fact]
        public void ValueProperty_UpdatesCorrectly()
        {
            var slider = new Slider(new Rectangle(0, 0, 100, 10), 0f, 10f, 0f, "Test");
            bool eventRaised = false;
            slider.ValueChanged += (_, e) => eventRaised = true;
            slider.Value = 5f;
            Assert.True(eventRaised);
            Assert.Equal(5f, slider.Value);
        }
    }
}