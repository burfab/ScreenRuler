using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ZZControls.Utils {
    public class RenderArea : FrameworkElement {
        public Action<RenderArea,DrawingVisual, DrawingContext> CustomRenderingDelegate { get; set; }

        private ContainerVisual _containerVisual;
        private DrawingVisual _renderArea;

        public RenderArea() {
            // Create a ContainerVisual to hold DrawingVisual children.
            _containerVisual = new ContainerVisual();

            _renderArea = new DrawingVisual();
            _containerVisual.Children.Add(_renderArea);
            
            // Create parent-child relationship with host visual and ContainerVisual.
            this.AddVisualChild(_containerVisual);
        }

        public void Render() {
            var ctx = _renderArea.RenderOpen();
            CustomRenderingDelegate?.Invoke(this,_renderArea, ctx);
            ctx.Close();
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            Render();
        }
        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount {
            get { return _containerVisual == null ? 0 : 1; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index) {
            if (_containerVisual == null) {
                throw new ArgumentOutOfRangeException();
            }

            return _containerVisual;
        }
    }
}
