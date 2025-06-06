﻿using FontStashSharp;
using Prowl.Quill;
using Prowl.Vector;
using System.Drawing;

namespace Common
{
    internal class BenchmarkScene
    {
        private Canvas _canvas;
        private double _width;
        private double _height;
        private double _time;

        // Benchmark parameters
        private const int RECT_COUNT = 40000;
        private const int CIRCLE_COUNT = 40000;
        private const double MAX_SIZE = 20.0;
        private const double MIN_SIZE = 5.0;

        // Performance tracking
        private Queue<double> _frameTimeHistory = new Queue<double>();
        private const int MAX_HISTORY_SAMPLES = 60;
        private double _averageFrameTime = 0;

        private SpriteFontBase _font;

        public BenchmarkScene(Canvas canvas, SpriteFontBase font)
        {
            _canvas = canvas;
            _font = font;
        }

        public void RenderFrame(double deltaTime, double width, double height)
        {
            _time += deltaTime;

            _width = width;
            _height = height;

            // Track performance
            UpdatePerformanceMetrics(deltaTime);

            // Clear the canvas
            _canvas.ResetState();

            // Run the benchmark
            DrawBenchmarkShapes();

            // Draw performance overlay
            DrawPerformanceOverlay();
        }

        private uint _randomState = 42;

        private float NextFloat()
        {
            _randomState = _randomState * 1103515245u + 12345u;
            return (_randomState >> 8) * (1.0f / 16777216.0f);
        }

        private void DrawBenchmarkShapes()
        {
            // Move Random outside and reuse - creating Random is expensive
            //var _random = new System.Random(42);
            _randomState = 42;

            // Precompute time-dependent values once
            double timeComponent = _time * 30;
            double colorTimeR = _time * 2;
            double colorTimeG = _time * 1.5;
            double colorTimeB = _time * 1.8;

            // Cache size range
            double sizeRange = MAX_SIZE - MIN_SIZE;
            double radiusMin = MIN_SIZE / 2;
            double radiusRange = (MAX_SIZE - MIN_SIZE) / 2;

            _canvas.SaveState();

            // Draw rectangles
            for (int i = 0; i < RECT_COUNT; i++)
            {
                // Precompute values instead of calling multiple times
                double x = NextFloat() * _width;
                double y = NextFloat() * _height;
                double rotation = (NextFloat() * 360) + (timeComponent * (i % 10));
                double scale = 0.5 + NextFloat() * 1.5;
                double width = MIN_SIZE + NextFloat() * sizeRange;
                double height = MIN_SIZE + NextFloat() * sizeRange;

                // Optimize color calculations
                double iOffset = i * 0.01;
                byte r = (byte)(128 + 127 * Math.Sin(colorTimeR + iOffset));
                byte g = (byte)(128 + 127 * Math.Sin(colorTimeG + i * 0.015));
                byte b = (byte)(128 + 127 * Math.Sin(colorTimeB + i * 0.008));

                _canvas.CurrentTransform(Transform2D.CreateTranslation(x, y));
                _canvas.TransformBy(Transform2D.CreateRotate(rotation));
                _canvas.TransformBy(Transform2D.CreateScale(scale, scale));
                _canvas.RectFilled(-width / 2, -height / 2, width, height, Color.FromArgb(180, r, g, b));
            }

            double circleColorTimeR = _time * 1.2;
            double circleColorTimeG = _time * 2.1;
            double circleColorTimeB = _time * 1.7;
            double scaleTime = _time * 3;

            for (int i = 0; i < CIRCLE_COUNT; i++)
            {
                double x = NextFloat() * _width;
                double y = NextFloat() * _height;
                double baseScale = 0.3 + NextFloat() * 1.2;
                double animScale = 1.0 + 0.2 * Math.Sin(scaleTime + i * 0.02);
                double totalScale = baseScale * animScale;
                double radius = radiusMin + NextFloat() * radiusRange;

                double iOffset = i * 0.012;
                byte r = (byte)(128 + 127 * Math.Cos(circleColorTimeR + iOffset));
                byte g = (byte)(128 + 127 * Math.Cos(circleColorTimeG + i * 0.008));
                byte b = (byte)(128 + 127 * Math.Cos(circleColorTimeB + i * 0.020));

                _canvas.CurrentTransform(Transform2D.CreateTranslation(x, y));
                _canvas.TransformBy(Transform2D.CreateScale(totalScale, totalScale));
                _canvas.CircleFilled(0, 0, radius, Color.FromArgb(160, r, g, b));
            }

            _canvas.RestoreState();
        }

        private void UpdatePerformanceMetrics(double deltaTime)
        {
            _frameTimeHistory.Enqueue(deltaTime * 1000); // Convert to milliseconds

            if (_frameTimeHistory.Count > MAX_HISTORY_SAMPLES)
                _frameTimeHistory.Dequeue();

            _averageFrameTime = _frameTimeHistory.Count > 0 ? _frameTimeHistory.Average() : 0;
        }

        private void DrawPerformanceOverlay()
        {
            // Performance overlay background
            double overlayWidth = 300;
            double overlayHeight = 80;
            double padding = 10;
            double x = _width - overlayWidth - padding;
            double y = padding;

            _canvas.RectFilled(x, y, overlayWidth, overlayHeight, Color.FromArgb(255, 0, 0, 0));

            // Border
            _canvas.SetStrokeColor(Color.FromArgb(255, 100, 100, 100));
            _canvas.SetStrokeWidth(1);
            _canvas.Rect(x, y, overlayWidth, overlayHeight);
            _canvas.Stroke();

            // Performance text
            double fps = _averageFrameTime > 0 ? 1000.0 / _averageFrameTime : 0;
            string perfText = $"BENCHMARK: {RECT_COUNT + CIRCLE_COUNT} SHAPES";
            string fpsText = $"FPS: {fps:F1}";
            string frameTimeText = $"Frame Time: {_averageFrameTime:F2} ms";

            // Simple text drawing (you'll need to implement this based on your font system)
            DrawSimpleText(perfText, x + 10, y + 15, Color.FromArgb(255, 255, 255, 100));
            DrawSimpleText(fpsText, x + 10, y + 35, Color.FromArgb(255, 100, 255, 100));
            DrawSimpleText(frameTimeText, x + 10, y + 55, Color.White);

            Console.Title = $"FPS: {fps:F1} | Frame Time: {_averageFrameTime:F2} ms | Shapes: {RECT_COUNT + CIRCLE_COUNT}";
        }

        // Simple text drawing method - you can replace this with your actual text rendering
        private void DrawSimpleText(string text, double x, double y, Color color)
        {
            // This is a placeholder - replace with your actual text rendering method
            // For example, if you have VectorFont like in the original code:
            // VectorFont.DrawString(_canvas, text, x, y, 12, color, 1f);
            _canvas.DrawText(_font, text, x, y, color);
        }
    }
}
