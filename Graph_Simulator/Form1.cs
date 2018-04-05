using RouteEngine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graph_Simulator
{
    public partial class Form1 : Form
    {
        private bool _isInAddMode = false;
        private bool _isRandomCostForNewConnection = false;
        private List<GuiNode> _guiNodes = new List<GuiNode>();
        private List<Connection> _conns = new List<Connection>();

        private GuiNode _selectedNode = null;
        private Color _normalColor;

        public Form1()
        {
            InitializeComponent();
            _normalColor = btnAddNode.BackColor;
        }

        private void btnAddNode_Click(object sender, EventArgs e)
        {
            btnAddNode.BackColor = _isInAddMode ? _normalColor : Color.Red;
            _isInAddMode = !_isInAddMode;
        }

        private void pnlView_MouseDown(object sender, MouseEventArgs e)
        {
            if (_isInAddMode)
            {
                var countNode = _guiNodes.Count;
                if (countNode > 25)
                {
                    MessageBox.Show("Maybe there are too much for a good simulation");
                    return;
                }
                if (GetGuiNodeAtPoint(e.X, e.Y) is null)
                {
                    var node = new GuiNode
                    {
                        Identifier = RouteEngine.RouteEngine.GenerateIdentifier(countNode),
                        X = e.X,
                        Y = e.Y
                    };
                    _guiNodes.Add(node);
                    cmbNodes.Items.Add(node);
                }
            }
            else
            {
                var node = GetGuiNodeAtPoint(e.X, e.Y);
                if (node is null)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        _selectedNode.Selected = false;
                        _selectedNode = null;
                        PaintGui();
                    }
                    return;
                }
                if (_selectedNode != null)
                {
                    var cost = _isRandomCostForNewConnection
                        ? new Random().Next(1, 25)
                        : int.Parse(txtCost.Text);

                    var conn = new Connection { A = _selectedNode, B = node, Cost = cost, IsTwoWay = chk2way.Checked };
                    _conns.Add(conn);
                    if (conn.IsTwoWay)
                    {
                        var reverse = new Connection { A = node, B = _selectedNode, Cost = cost, IsTwoWay = true };
                        _conns.Add(reverse);
                    }
                    _selectedNode.Selected = false;
                    _selectedNode = null;
                }
                else
                {
                    node.Selected = true;
                    _selectedNode = node;
                }
            }
            PaintGui();
        }

        private void PaintGui()
        {
            var _brushRed = new SolidBrush(Color.Red);
            var _brushBlack = new SolidBrush(Color.Black);
            var _brushWhite = new SolidBrush(Color.White);
            var _brushBlue = new SolidBrush(Color.Blue);
            var _font = new Font(FontFamily.GenericSansSerif, 15);
            var _penBlue = new Pen(_brushBlue);
            var _penRed = new Pen(_brushRed);

            foreach (var node in _guiNodes)
            {
                var posX = node.X - node.Diameter / 2;
                var posY = node.Y - node.Diameter / 2;

                if (node.Selected)
                    pnlView.CreateGraphics().FillEllipse(_brushRed, posX, posY, node.Diameter, node.Diameter);
                else
                    pnlView.CreateGraphics().FillEllipse(_brushBlack, posX, posY, node.Diameter, node.Diameter);
                pnlView.CreateGraphics().DrawString(node.Identifier.ToString(), _font, _brushWhite, posX + node.Diameter / 4, posY + node.Diameter / 8);
            }

            foreach (var conn in _conns)
            {
                var point1 = new Point(((GuiNode)conn.A).X, ((GuiNode)conn.A).Y);
                var point2 = new Point(((GuiNode)conn.B).X, ((GuiNode)conn.B).Y);

                var Pointref = Point.Subtract(point2, new Size(point1));
                var degrees = Math.Atan2(Pointref.Y, Pointref.X);
                var cosx1 = Math.Cos(degrees);
                var siny1 = Math.Sin(degrees);

                var cosx2 = Math.Cos(degrees + Math.PI);
                var siny2 = Math.Sin(degrees + Math.PI);

                var newx = (int)(cosx1 * (float)((GuiNode)conn.A).Diameter + (float)point1.X);
                var newy = (int)(siny1 * (float)((GuiNode)conn.A).Diameter + (float)point1.Y);

                var newx2 = (int)(cosx2 * (float)((GuiNode)conn.B).Diameter + (float)point2.X);
                var newy2 = (int)(siny2 * (float)((GuiNode)conn.B).Diameter + (float)point2.Y);
                
                if (conn.Selected)
                {
                    pnlView.CreateGraphics().DrawLine(_penRed, new Point(newx, newy), new Point(newx2, newy2));
                    pnlView.CreateGraphics().DrawString(conn.Cost.ToString(), _font, _brushRed, newx - 4, newy - 4);
                    if (!conn.IsTwoWay)
                        pnlView.CreateGraphics().FillEllipse(_brushRed, newx - 4, newy - 4, 8, 8);
                }
                else
                {
                    pnlView.CreateGraphics().DrawLine(_penBlue, new Point(newx, newy), new Point(newx2, newy2));
                    pnlView.CreateGraphics().DrawString(conn.Cost.ToString(), _font, _brushBlue, newx - 4, newy - 4);
                    if (!conn.IsTwoWay)
                        pnlView.CreateGraphics().FillEllipse(_brushBlue, newx - 4, newy - 4, 8, 8);
                }
            }
        }

        private GuiNode GetGuiNodeAtPoint (int x, int y)
        {
            foreach (var node in _guiNodes)
            {
                var x2 = x - node.X;
                var y2 = y - node.Y;
                var xToCompare = node.Diameter / 2;
                var yToCompare = node.Diameter / 2;

                if (x2 >= xToCompare * -1 && x2 < xToCompare && y2 > yToCompare * -1 && y2 < yToCompare)
                {
                    return node;
                }
            }

            return null;
        }

        private void pnlView_Paint(object sender, PaintEventArgs e)
        {
            PaintGui();
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            if (cmbNodes.SelectedIndex != -1)
            {
                var _routeEngine = new RouteEngine.RouteEngine();
                foreach (var connection in _conns)
                {
                    _routeEngine.Connections.Add(connection);
                }

                foreach (var node in _guiNodes)
                {
                    _routeEngine.Nodes.Add(node);
                }

                var _shortestPaths = _routeEngine.CalculateMinCost((Node)cmbNodes.SelectedItem);
                lstResult.Items.Clear();

                foreach (var node in _shortestPaths.OrderBy(p => p.Value.TotalCost).Select(p => p.Key))
                {
                    lstResult.Items.Add(_shortestPaths[node]);
                }
            }
            else
            {
                MessageBox.Show("Please select a position");
            }
        }

        private void lstResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            var route = (Route)lstResult.SelectedItem;
            foreach (var conn in _conns)
            {
                conn.Selected = false;
            }

            foreach (var conn in route.Connections)
            {
                conn.Selected = true;
            }
            PaintGui();
        }

        private void chkRandom_CheckedChanged(object sender, EventArgs e)
        {
            _isRandomCostForNewConnection = chkRandom.Checked;
            txtCost.Enabled = !_isRandomCostForNewConnection;
        }
    }
}
