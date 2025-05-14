using Common;
using Prowl.Quill;
using Prowl.Vector;
using System.Drawing;

namespace OpenTKSVG
{
    class SVGDemo
    {
        private Canvas _canvas;
        private Canvas3D _canvas3D;
        private double _width;
        private double _height;

        private double _time;

        // Demo state
        private double _rotation = 0f;

        // Performance monitoring
        private Queue<double> _frameTimeHistory = new Queue<double>();
        private Queue<double> _fpsHistory = new Queue<double>();
        private const int MAX_HISTORY_SAMPLES = 100;
        private double _fpsUpdateCounter = 0;
        private double _currentFps = 0;
        private const double FPS_UPDATE_INTERVAL = 0.5; // Update FPS display every half second


        public SVGDemo(Canvas canvas, double width, double height)
        {
            _canvas = canvas;
            _width = width;
            _height = height;
            _canvas3D = new Canvas3D(canvas, width, height);
        }

        /// <summary>
        /// Updates and renders a frame
        /// </summary>
        public void RenderFrame(double deltaTime, Vector2 offset, double zoom, double rotate)
        {
            // Update time
            _time += deltaTime;

            // Update performance metrics
            UpdatePerformanceMetrics(deltaTime);

            // Update rotation based on time
            _rotation += deltaTime * 30f; // 30 degrees per second

            _canvas.TransformBy(Transform2D.CreateTranslation(_width / 2, _height / 2));
            _canvas.TransformBy(Transform2D.CreateRotate(rotate) * Transform2D.CreateTranslation(offset.x, offset.y) * Transform2D.CreateScale(zoom, zoom));
            _canvas.SetStrokeScale(zoom);

            //_canvas.SetTexture(_texture);
            //_canvas.Scissor(0, 0, 200, 200);

            DrawDemo2D();

            _canvas.ResetState();

            // Draw performance overlay
            DrawPerformanceOverlay();
        }

        private void UpdatePerformanceMetrics(double deltaTime)
        {
            // Calculate FPS
            double instantFps = deltaTime > 0 ? 1.0 / deltaTime : 0;

            // Add to history queues, keeping a fixed size
            _frameTimeHistory.Enqueue(deltaTime * 1000); // Convert to milliseconds
            _fpsHistory.Enqueue(instantFps);

            if (_frameTimeHistory.Count > MAX_HISTORY_SAMPLES)
                _frameTimeHistory.Dequeue();

            if (_fpsHistory.Count > MAX_HISTORY_SAMPLES)
                _fpsHistory.Dequeue();

            // Update the FPS counter at intervals to make it readable
            _fpsUpdateCounter += deltaTime;
            if (_fpsUpdateCounter >= FPS_UPDATE_INTERVAL)
            {
                _currentFps = _fpsHistory.Count > 0 ? _fpsHistory.Average() : 0;
                _fpsUpdateCounter = 0;
            }
        }

        private void DrawDemo2D()
        {
            // Save the canvas state
            _canvas.SaveState();

            DrawGrid(16, 17, 50, Color.FromArgb(40, 255, 255, 255));

            // Restore the canvas state
            _canvas.RestoreState();
        }

        private void DrawGrid(int x, int y, double cellSize, Color color)
        {
            _canvas.SetStrokeColor(color);
            _canvas.SetStrokeWidth(4);

            // Draw horizontal lines
            _canvas.BeginPath();
            for (int i = 0; i <= y; i++)
            {
                _canvas.MoveTo(0, i * cellSize);
                _canvas.LineTo((cellSize * x), i * cellSize);
            }
            _canvas.Stroke();

            // Draw vertical lines
            _canvas.BeginPath();
            for (int i = 0; i <= x; i++)
            {
                _canvas.MoveTo(i * cellSize, 0);
                _canvas.LineTo(i * cellSize, (cellSize * y));
            }
            _canvas.Stroke();
        }

        private void DrawPerformanceOverlay()
        {
            // Draw background for performance overlay
            double overlayWidth = 200;
            double overlayHeight = 120;
            double padding = 10;

            // Position in top-right corner with padding
            double x = _width - overlayWidth - padding;
            double y = padding;

            // Background with semi-transparency
            _canvas.RectFilled(x, y, overlayWidth, overlayHeight, Color.FromArgb(180, 0, 0, 0));

            // Draw border
            _canvas.SetStrokeColor(Color.FromArgb(255, 100, 100, 100));
            _canvas.SetStrokeWidth(1);
            _canvas.Rect(x, y, overlayWidth, overlayHeight);
            _canvas.Stroke();

            // Draw FPS counter
            string fpsText = $"FPS: {_currentFps:F1}";
            double frameTimeAvg = _frameTimeHistory.Count > 0 ? _frameTimeHistory.Average() : 0;
            string frameTimeText = $"Frame Time: {frameTimeAvg:F1} ms";

            DrawText(fpsText, x + 10, y + 20, 14, Color.FromArgb(255, 100, 255, 100));
            DrawText(frameTimeText, x + 10, y + 40, 12, Color.White);

            // Draw performance graph
            double graphX = x + 10;
            double graphY = y + 60;
            double graphWidth = overlayWidth - 20;
            double graphHeight = 50;

            // Draw graph background
            _canvas.RectFilled(graphX, graphY, graphWidth, graphHeight, Color.FromArgb(60, 255, 255, 255));

            //// Draw FPS graph
            //if (_fpsHistory.Count > 1)
            //{
            //    // Find max value to scale the graph (with minimum range of 0-60 FPS)
            //    double maxFps = Math.Max(60, _fpsHistory.Max());
            //
            //    _canvas.SetStrokeColor(Color.FromArgb(255, 100, 255, 100));
            //    _canvas.SetStrokeWidth(1.5);
            //    _canvas.BeginPath();
            //
            //    // Draw the FPS graph line
            //    bool first = true;
            //    double xStep = graphWidth / (MAX_HISTORY_SAMPLES - 1);
            //    int i = 0;
            //
            //    foreach (double fps in _fpsHistory)
            //    {
            //        double normalizedValue = fps / maxFps; // 0.0 to 1.0
            //        double pointX = graphX + (i * xStep);
            //        double pointY = graphY + graphHeight - (normalizedValue * graphHeight);
            //
            //        if (first)
            //        {
            //            _canvas.MoveTo(pointX, pointY);
            //            first = false;
            //        }
            //        else
            //        {
            //            _canvas.LineTo(pointX, pointY);
            //        }
            //
            //        i++;
            //    }
            //
            //    _canvas.Stroke();
            //
            //    // Draw the target 60 FPS line
            //    double targetY = graphY + graphHeight - ((60 / maxFps) * graphHeight);
            //    _canvas.SetStrokeColor(Color.FromArgb(100, 255, 100, 100));
            //    _canvas.SetStrokeWidth(1.0);
            //
            //    _canvas.BeginPath();
            //    _canvas.MoveTo(graphX, targetY);
            //    _canvas.LineTo(graphX + graphWidth, targetY);
            //    _canvas.Stroke();
            //}
        }

        private void DrawText(string text, double x, double y, double height, Color color)
        {
            VectorFont.DrawString(_canvas, text.ToUpper(), x, y, height, color, 2f);
        }
    }
}
