﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp {
    public partial class mainActivity : Form {
        private bool shouldDraw = true;
        private bool transformationsEnabled = false;
        private List<Point> pathPoints;
        private readonly drawingActivity childActivity;

        public mainActivity() {
            childActivity = new drawingActivity(this);
            InitializeComponent();
        }

        private void drawingBox_Paint(object sender, PaintEventArgs e) {
            Graphics world = e.Graphics;
            world.SmoothingMode = SmoothingMode.AntiAlias;

            decimal rotationAngle = rotationValue.Value;
            decimal homothesis = homothesisValue.Value;
            decimal translateX = translateXValue.Value;
            decimal translateY = translateYValue.Value;

            Size drawingBoxSize = drawingBox.Size;
            GraphicsPath xAxis = new GraphicsPath();
            GraphicsPath yAxis = new GraphicsPath();
            GraphicsPath path = new GraphicsPath();

            xAxis.AddLine(new Point(-drawingBoxSize.Width, 0), new Point(drawingBoxSize.Width, 0));
            yAxis.AddLine(new Point(0, -drawingBoxSize.Height), new Point(0, drawingBoxSize.Height));

            if (pathPoints != null && pathPoints.Count > 0)
                path.AddLines(pathPoints.ToArray());

            Matrix tf = new Matrix();

            if (pathPoints != null && pathPoints.Count > 0 && transformationsEnabled) {
                if (reflectionXCheckbox.Checked)
                    tf.Multiply(new Matrix(1, 0, 0, -1, 0, 0), MatrixOrder.Append);

                if (reflectionYCheckbox.Checked)
                    tf.Multiply(new Matrix(-1, 0, 0, 1, 0, 0), MatrixOrder.Append);

                if (rotationCheckbox.Checked)
                    tf.Rotate(-(float)rotationAngle, MatrixOrder.Append);

                if (homothesisCheckbox.Checked)
                    tf.Scale((float)homothesis, (float)homothesis, MatrixOrder.Append);

                if (translateCheckbox.Checked)
                    tf.Translate((float)translateX, -(float)translateY, MatrixOrder.Append);

            }

            updateMatrixValues(tf);

            world.TranslateTransform(drawingBoxSize.Width / 2, drawingBoxSize.Height / 2);
            world.DrawPath(new Pen(Color.Black, 1), xAxis);
            world.DrawPath(new Pen(Color.Black, 1), yAxis);

            if (pathPoints != null && pathPoints.Count > 0) {
                world.DrawPath(new Pen(Color.Green, 2), path);

                if (transformationsEnabled && shouldDraw) {
                    path.Transform(tf);
                    world.DrawPath(new Pen(Color.Blue, 2), path);
                }
            }


        }

        private void updateMatrixValues(Matrix matrix) {
            List<float> matrixValues = matrix.Elements.ToList();
            matrix11.Text = matrixValues[0].ToString();
            matrix12.Text = matrixValues[1].ToString();
            matrix21.Text = matrixValues[2].ToString();
            matrix22.Text = matrixValues[3].ToString();
            matrix31.Text = matrixValues[4].ToString();
            matrix32.Text = matrixValues[5].ToString();
        }

        private void refreshScreen_Event(object sender, EventArgs e) =>
            Refresh();

        private void numeric_ValueChanged(object sender, EventArgs e) {
            CheckBox checkbox;
            NumericUpDown textBox = sender as NumericUpDown;
            switch (textBox.Name) {
                case "rotationValue":
                    if (textBox.Value > (decimal)359.9 || textBox.Value < (decimal)-359.9)
                        textBox.Value = 0;

                    checkbox = rotationCheckbox;
                    break;
                case "homothesisValue":
                    if (textBox.Value == 0)
                        shouldDraw = false;
                    else
                        shouldDraw = true;

                    checkbox = homothesisCheckbox;
                    break;
                case "translateXValue":
                case "translateYValue":
                    checkbox = translateCheckbox;
                    break;
                default:
                    checkbox = new CheckBox();
                    break;
            }

            if (checkbox.Checked)
                Refresh();
        }

        private void applyTransformations_CheckedChanged(object sender, EventArgs e) {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.Checked && !transformationsEnabled) {
                transformationsEnabled = true;
            } else if (transformationsEnabled && !isAnyChecked()) {
                transformationsEnabled = false;
            }
            Refresh();
        }

        private bool isAnyChecked() {
            return (
                rotationCheckbox.Checked ||
                homothesisCheckbox.Checked ||
                reflectionXCheckbox.Checked ||
                reflectionYCheckbox.Checked ||
                translateCheckbox.Checked
            );
        }

        private void cleanTransformBtn_Click(object sender, EventArgs e) {
            transformationsEnabled = false;
            reflectionXCheckbox.Checked = false;
            reflectionYCheckbox.Checked = false;
            translateCheckbox.Checked = false;
            translateXValue.Value = 0;
            translateYValue.Value = 0;
            rotationCheckbox.Checked = false;
            rotationValue.Value = 0;
            homothesisCheckbox.Checked = false;
            homothesisValue.Value = 1;
        }

        private void editDrawBtn_Click(object sender, EventArgs e) {
            if (!childActivity.Visible)
                childActivity.Show();
            else
                _ = childActivity.Focus();
        }

        public void handleChildValues() {
            pathPoints = childActivity.pathPoints;
            Refresh();
        }
    }
}
