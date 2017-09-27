﻿/*!
The MIT License (MIT)

Copyright (c) 2017 Dimitris Katikaridis <dkatikaridis@gmail.com>,Giannis Menekses <johnmenex@hotmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System;
using System.Drawing;
using System.Windows.Forms;


namespace kagv {

    public partial class main_form : Form {


        //custom constructor of this form.
        public main_form() {
            InitializeComponent();//Create the form layout
            initialization();//initialize our stuff
        }

        //paint event on form.
        //This event is triggered when a paint event or mouse event is happening over the form.
        //mouse clicks ,hovers and clicks are also considered as triggers
        private void main_form_Paint(object sender, PaintEventArgs e) {
            paper = e.Graphics;
            paper.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            
            try {
                if (importedLayout != null) {
                   
                    Rectangle r = new Rectangle (new Point (m_rectangles[0][0].x,m_rectangles[0][0].y)
                        , new Size((m_rectangles[Constants.__WidthBlocks-1][Constants.__HeightBlocks-1].x)+Constants.__BlockSide
                            , (m_rectangles[Constants.__WidthBlocks - 1][Constants.__HeightBlocks - 1].y) - Constants.__TopBarOffset + Constants.__BlockSide));
                    paper.DrawImage(importedLayout, r);
                   
                }
                //draws the grid
                for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++) {
                    for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++) {
                        //show the relative box color regarding the box type we have chose
                        m_rectangles[widthTrav][heightTrav].DrawBox(paper, BoxType.Normal); 
                        m_rectangles[widthTrav][heightTrav].DrawBox(paper, BoxType.Start);
                        m_rectangles[widthTrav][heightTrav].DrawBox(paper, BoxType.End);
                        m_rectangles[widthTrav][heightTrav].DrawBox(paper, BoxType.Wall);
                        m_rectangles[widthTrav][heightTrav].DrawBox(paper, BoxType.Load);

                        if (m_rectangles[widthTrav][heightTrav].boxType == BoxType.Load
                            && isLoad[widthTrav, heightTrav] == 3)
                            m_rectangles[widthTrav][heightTrav].SetAsTargetted(paper);
                    }
                }

                int c = 0;
                for (int i = 0; i < StartPos.Count; i++) //count how much agvs we have added to the grid
                    c += AGVs[i].JumpPoints.Count; //...and add them in a variable

                for (int i = 0; i < StartPos.Count; i++) {
                    AGVs[i].StepsCounter = 0;

                    if (!NoJumpPointsFound)//if jump points found...
                        for (int resultTrav = 0; resultTrav < c; resultTrav++)
                            try {
                                if (linesToolStripMenuItem.Checked)
                                    AGVs[i].Paths[resultTrav].drawLine(paper);//draw the lines 
                                if (!isMouseDown)
                                    DrawPoints(AGVs[i].Paths[resultTrav], i);//show points
                            } catch { }
                }

                //handle the red message above every agv
                int AGVs_list_index = 0;
                if (aGVIndexToolStripMenuItem.Checked)
                    for (int i = 0; i < nUD_AGVs.Value; i++)
                        if (!TrappedStatus[i]) {
                            paper.DrawString("AGV:" + AGVs[AGVs_list_index].ID,
                                             new Font("Tahoma", 8, FontStyle.Bold),
                                             new SolidBrush(Color.Red),
                                             new Point((StartPos[AGVs_list_index].x * Constants.__BlockSide) - 10, ((StartPos[AGVs_list_index].y * Constants.__BlockSide) + Constants.__TopBarOffset) - Constants.__BlockSide));
                            AGVs_list_index++;
                        }    
                        
            } catch { }
        }

        private void main_form_Load(object sender, EventArgs e) {

            //Load all values

            //Transparent and SemiTransparent feature serves the agri/industrial branch recursively
            importImageLayoutToolStripMenuItem.Enabled = Constants.__SemiTransparency;

            if (importImageLayoutToolStripMenuItem.Enabled)
                importImageLayoutToolStripMenuItem.Text = "Import image layout";
            else
                importImageLayoutToolStripMenuItem.Text = "Semi Transparency feature is disabled";

            //Reflection here is not actually a C# Reflection but a mirroring of some apps
            if (!reflected) {
                reflectedWidth = Constants.__WidthBlocks;
                reflectedHeight = Constants.__HeightBlocks;
                reflectedBlock = Constants.__BlockSide;
                reflected = true;

                stepsToolStripMenuItem.Checked = false;
                linesToolStripMenuItem.Checked =
                dotsToolStripMenuItem.Checked =
                bordersToolStripMenuItem.Checked =
                aGVIndexToolStripMenuItem.Checked =
                highlightOverCurrentBoxToolStripMenuItem.Checked = true;
            }

            Text = "K-aGv2 Simulator (Industrial branch)";

            var _proc = System.Diagnostics.Process.GetCurrentProcess();
            _proc.ProcessorAffinity = new IntPtr(0x0003);//use cores 1,2 
            //ptr flag has to be (bin) 0011 so its IntPtr 0x0003
            //More infos here:https://msdn.microsoft.com/en-us/library/system.diagnostics.processthread.processoraffinity(v=vs.110).aspx

            refresh_label.Text = "Delay :" + timer0.Interval + " ms";

            nUD_AGVs.Value = 0;

            agv1steps_LB.Text =
            agv2steps_LB.Text =
            agv3steps_LB.Text =
            agv4steps_LB.Text =
            agv5steps_LB.Text = "";

            //Do not show the START menu because there is no valid path yet
            triggerStartMenu(false);

            rb_start.Checked = true;
            BackColor = Color.DarkGray;

            CenterToScreen();

            useRecursiveToolStripMenuItem.Checked = useRecursive;
            crossCornerToolStripMenuItem.Checked = crossCorners;
            crossAdjacentPointToolStripMenuItem.Checked = crossAdjacent;
            manhattanToolStripMenuItem.Checked = true;

            //dynamically add the location of menupanel.
            //We have to do it dynamically because the forms size is always depended on PCs actual screen size
            menuPanel.Location = new Point(0, 24 + 1);//24=menu bar Y
            menuPanel.Width = Width;

            tp = new ToolTip
            {

                AutomaticDelay = 0,
                ReshowDelay = 0,
                InitialDelay = 0,
                AutoPopDelay = 0,
                IsBalloon = true,
                ToolTipIcon = ToolTipIcon.Info,
                ToolTipTitle = "Grid Block Information",
            };

        }

        private void main_form_MouseDown(object sender, MouseEventArgs e) {
            //If the simulation is running, do not do anything.leave the function explicitly
            if (timer0.Enabled || timer1.Enabled || timer2.Enabled || timer3.Enabled || timer4.Enabled)
                return;

            //Supposing that timers are not enabled(that means that the simulation is not running)
            //we have a clicked point.Check if that point is valid.if not explicitly leave
            Point _validationPoint = new Point(e.X, e.Y);
            if (!isvalid(_validationPoint))
                return;

            //if the clicked point is inside a rectangle...
            isMouseDown = true;
            if ((e.Button == MouseButtons.Left) && (rb_wall.Checked))
                for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                    for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                        if (m_rectangles[widthTrav][heightTrav].boxRec.IntersectsWith(new Rectangle(e.Location, new Size(1, 1)))) {
                            m_lastBoxType = m_rectangles[widthTrav][heightTrav].boxType;
                            m_lastBoxSelect = m_rectangles[widthTrav][heightTrav];
                            switch (m_lastBoxType) { //...measure the reaction
                                case BoxType.Normal: //if its wall or normal ,switch it to the opposite.
                                case BoxType.Wall:
                                    m_rectangles[widthTrav][heightTrav].SwitchBox();
                                    Invalidate();
                                    break;
                                case BoxType.Start: //if its start or end,do nothing.
                                case BoxType.End:
                                    break;
                            }
                        }
            //if the user press the right button of the mouse
            if (e.Button == MouseButtons.Right) {

                //prepare the tooltip
                Point mycoords = new Point(e.X, e.Y);
                GridBox clickedBox = null;
                bool isBorder = true;
                string currentBoxCoords = "X: - Y: -\r\n";
                string currentBoxIndex = "Index: N/A\r\n";
                string currentBoxType = "Block type: Border\r\n";
                string isPath = "Is part of path: N/A\r\n";
                bool isPathBlock = false;
                for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                    for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                        if (m_rectangles[widthTrav][heightTrav].boxRec.Contains(mycoords)) {
                            currentBoxType =
                                "Block type: " +
                                m_rectangles[widthTrav][heightTrav].boxType + "\r\n";
                            currentBoxCoords =
                                "X: " +
                                m_rectangles[widthTrav][heightTrav].boxRec.X + " " +
                                "Y: " +
                                m_rectangles[widthTrav][heightTrav].boxRec.Y + "\r\n";
                            currentBoxIndex =
                                "Index: " +
                                "iX: " + widthTrav + " " + "iY: " + heightTrav + "\r\n";

                            int agv_index = 0;

                            if (StartPos != null) {
                                for (int j = 0; j < StartPos.Count; j++)
                                    for (int i = 0; i < Constants.__MaximumSteps; i++)
                                        if (m_rectangles[widthTrav][heightTrav].boxRec.Contains
                                            (
                                                   new Point(
                                                       Convert.ToInt32(AGVs[j].Steps[i].X),
                                                       Convert.ToInt32(AGVs[j].Steps[i].Y)
                                                       )
                                            )) {
                                            isPathBlock = true;
                                            agv_index = j;
                                            i = Constants.__MaximumSteps;
                                            j = StartPos.Count;
                                        }
                                clickedBox = m_rectangles[widthTrav][heightTrav];
                            }
                            tp.ToolTipIcon = ToolTipIcon.Info;
                            if (isPathBlock && StartPos != null) {
                                isPath = "Is part of AGV"+(agv_index)+" path";
                                tp.Show(currentBoxType + currentBoxCoords + currentBoxIndex + isPath
                                    , this
                                    , clickedBox.boxRec.X
                                    , clickedBox.boxRec.Y - Constants.__TopBarOffset + 17);
                                isBorder = false;
                            } else {
                                isPath = "Is part of path:No\r\n";
                                clickedBox = new GridBox(e.X, e.Y, BoxType.Normal);
                                //show the tooltip
                                tp.Show(currentBoxType + currentBoxCoords + currentBoxIndex + isPath
                                    , this
                                    , clickedBox.boxRec.X - 10
                                    , clickedBox.boxRec.Y - Constants.__TopBarOffset + 12);
                                isBorder = false;
                            }
                        }
                    
                

                if (isBorder) {
                    tp.ToolTipIcon = ToolTipIcon.Error;
                    //show the tooltip
                    tp.Show(currentBoxType + currentBoxCoords + currentBoxIndex + isPath
                               , this
                               , e.X - 8
                               , e.Y - Constants.__TopBarOffset + 14);
                }

            }

        }

        private void main_form_MouseMove(object sender, MouseEventArgs e) {
            //this event is triggered when the mouse is moving above the form

            //if we hold the left click and the Walls setting is selected....
            if (isMouseDown && rb_wall.Checked) {
                if (e.Button == MouseButtons.Left) {
                    if (m_lastBoxSelect.boxType == BoxType.Start ||
                        m_lastBoxSelect.boxType == BoxType.End)
                        return;

                    //that IF() means: if my click is over an already drawn box...
                    if (m_lastBoxSelect == null) {
                        for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++) {
                            for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++) {
                                if (m_rectangles[widthTrav][heightTrav].boxRec.IntersectsWith(new Rectangle(e.Location, new Size(1, 1)))) {
                                    m_lastBoxType = m_rectangles[widthTrav][heightTrav].boxType;
                                    m_lastBoxSelect = m_rectangles[widthTrav][heightTrav];
                                    switch (m_lastBoxType) {
                                        case BoxType.Normal:
                                        case BoxType.Wall:
                                            m_rectangles[widthTrav][heightTrav].SwitchBox(); //switch it if needed...
                                            Invalidate();
                                            break;
                                        case BoxType.Start:
                                        case BoxType.End:
                                            break;
                                    }
                                }

                            }
                        }

                        return;
                        //else...its a new/fresh box
                    } else {
                        for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++) {
                            for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++) {
                                if (m_rectangles[widthTrav][heightTrav].boxRec.IntersectsWith(new Rectangle(e.Location, new Size(1, 1)))) {
                                    if (m_rectangles[widthTrav][heightTrav] == m_lastBoxSelect) {
                                        return;
                                    } else {

                                        switch (m_lastBoxType) {
                                            case BoxType.Normal:
                                            case BoxType.Wall:
                                                if (m_rectangles[widthTrav][heightTrav].boxType == m_lastBoxType) {
                                                    m_rectangles[widthTrav][heightTrav].SwitchBox();
                                                    m_lastBoxSelect = m_rectangles[widthTrav][heightTrav];
                                                    Invalidate();
                                                }
                                                break;
                                            case BoxType.Start:
                                                m_lastBoxSelect.SetNormalBox();
                                                m_lastBoxSelect = m_rectangles[widthTrav][heightTrav];
                                                Invalidate();
                                                break;
                                            case BoxType.End:
                                                m_lastBoxSelect.SetNormalBox();
                                                m_lastBoxSelect = m_rectangles[widthTrav][heightTrav];
                                                m_lastBoxSelect.SetEndBox();
                                                Invalidate();
                                                break;
                                        }
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (
                 timer0.Enabled ||
                 timer1.Enabled ||
                 timer2.Enabled ||
                 timer3.Enabled ||
                 timer4.Enabled
               )
                return;
            
            //if user enable the highlighting over a box while mouse hovering
            if (allowHighlight)
                for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                    for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                        if (m_rectangles[widthTrav][heightTrav].boxRec.Contains(new Point(e.X, e.Y))
                            && m_rectangles[widthTrav][heightTrav].boxType == BoxType.Normal) {
                            if (rb_load.Checked)
                                m_rectangles[widthTrav][heightTrav].onHover(Color.FromArgb(150, Color.FromArgb(138, 109, 86)));
                            else if (rb_start.Checked)
                                m_rectangles[widthTrav][heightTrav].onHover(Color.LightGreen);
                            else if (rb_stop.Checked)
                                m_rectangles[widthTrav][heightTrav].onHover(Color.FromArgb(80, Color.FromArgb(255, 26, 26)));
                            else //wall
                                m_rectangles[widthTrav][heightTrav].onHover(Color.FromArgb(20, Color.LightGray));

                            Invalidate();
                        } else if (m_rectangles[widthTrav][heightTrav].boxType == BoxType.Normal) {
                            m_rectangles[widthTrav][heightTrav].onHover(boxDefaultColor);
                            Invalidate();
                        }
        }

        //The most important event.When we let our mouse click up,all our changes are
        //shown in the screen
        private void main_form_MouseUp(object sender, MouseEventArgs e) {


            if (timer0.Enabled 
                || timer1.Enabled 
                || timer2.Enabled 
                || timer3.Enabled 
                || timer4.Enabled) return;

            isMouseDown = false;

            for (int i = 0; i < StartPos.Count; i++)
                AGVs[i].StepsCounter = 0;

            if (e.Button == MouseButtons.Right)
                tp.Hide(this);
            
            Redraw();//The main function of this executable.Contains almost every drawing and calculating stuff
            Invalidate();
        }


        private void nUD_AGVs_ValueChanged(object sender, EventArgs e) {

            //if we change the AGVs value from numeric updown,do the following
            int starts_counter = 0;
            bool removed = false;
            int[,] starts_position = new int[2, Convert.ToInt32(nUD_AGVs.Value) + 1]; //keeps the size of the array +1 in relation with the nUD

            for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++) {
                    if (m_rectangles[widthTrav][heightTrav].boxType == BoxType.Start) {
                        starts_position[0, starts_counter] = widthTrav;
                        starts_position[1, starts_counter] = heightTrav;
                        starts_counter++;
                    }
                    //if we reduce the numeric value and become less than the already-drawn agvs,remove the rest agvs
                    if (starts_counter > nUD_AGVs.Value) {
                        m_rectangles[starts_position[0, starts_counter - 1]][starts_position[1, starts_counter - 1]].SwitchEnd_StartToNormal(); //removes the very last
                        
                        removed = true;
                        Invalidate();
                    }
                }
            if (removed)
                Redraw();
        }

        private void main_form_MouseClick(object sender, MouseEventArgs e) {

            if (timer0.Enabled 
                || timer1.Enabled 
                || timer2.Enabled 
                || timer3.Enabled 
                || timer4.Enabled) return;


            Point click_coords = new Point(e.X, e.Y);
            if (!isvalid(click_coords) || e.Button != MouseButtons.Left || nUD_AGVs.Value == 0)
                return;

            if (rb_load.Checked)
                for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                    for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                        if (m_rectangles[widthTrav][heightTrav].boxRec.IntersectsWith(new Rectangle(e.Location, new Size(1, 1)))) {
                            m_lastBoxType = m_rectangles[widthTrav][heightTrav].boxType;
                            m_lastBoxSelect = m_rectangles[widthTrav][heightTrav];
                            switch (m_lastBoxType) {
                                case BoxType.Normal:
                                    //loads++;
                                    m_rectangles[widthTrav][heightTrav].SwitchLoad();
                                    isLoad[widthTrav, heightTrav] = 1;
                                    Invalidate();
                                    break;
                                case BoxType.Load:
                                    loads--;
                                    m_rectangles[widthTrav][heightTrav].SwitchLoad();
                                    isLoad[widthTrav, heightTrav] = 2;
                                    Invalidate();
                                    break;
                                case BoxType.Wall:
                                case BoxType.Start:
                                case BoxType.End:
                                    break;
                            }
                        }

            if (rb_start.Checked) {

                if (nUD_AGVs.Value == 1)//Saves only the last Click position to place the Start (1 start exists)
                {
                    for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                        for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                            if (m_rectangles[widthTrav][heightTrav].boxType == BoxType.Start)
                                m_rectangles[widthTrav][heightTrav].SwitchEnd_StartToNormal();
                } else if (nUD_AGVs.Value > 1)//Deletes the start with the smallest iX - iY coords and keeps the rest
                {
                    int starts_counter = 0;
                    int[,] starts_position = new int[2, Convert.ToInt32(nUD_AGVs.Value)];


                    for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                        for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++) {
                            if (m_rectangles[widthTrav][heightTrav].boxType == BoxType.Start) {
                                starts_position[0, starts_counter] = widthTrav;
                                starts_position[1, starts_counter] = heightTrav;
                                starts_counter++;
                            }
                            if (starts_counter == nUD_AGVs.Value) {
                                m_rectangles[starts_position[0, 0]][starts_position[1, 0]].SwitchEnd_StartToNormal();
                            }
                        }
                }
                
                //Converts the clicked box to Start point
                for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                    for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                        if (m_rectangles[widthTrav][heightTrav].boxRec.Contains(click_coords)
                         && m_rectangles[widthTrav][heightTrav].boxType == BoxType.Normal)
                                m_rectangles[widthTrav][heightTrav] = new GridBox(widthTrav * Constants.__BlockSide, heightTrav * Constants.__BlockSide + Constants.__TopBarOffset, BoxType.Start);
                
            }
            //same for Stop
            if (rb_stop.Checked) {
                for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                    for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                        if (m_rectangles[widthTrav][heightTrav].boxType == BoxType.End)
                            m_rectangles[widthTrav][heightTrav].SwitchEnd_StartToNormal();//allow only one end point


                for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                    for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                        if (m_rectangles[widthTrav][heightTrav].boxRec.Contains(click_coords)
                             &&
                            m_rectangles[widthTrav][heightTrav].boxType == BoxType.Normal) {
                                m_rectangles[widthTrav][heightTrav] = new GridBox(widthTrav * Constants.__BlockSide, heightTrav * Constants.__BlockSide + Constants.__TopBarOffset, BoxType.End);
                        }
            }
            
            Invalidate();
        }
        //parametres
        private void useRecursiveToolStripMenuItem_Click(object sender, EventArgs e) {
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;
            useRecursive = (sender as ToolStripMenuItem).Checked;
            updateParameters();
            Redraw();

        }

        private void crossAdjacentPointToolStripMenuItem_Click(object sender, EventArgs e) {
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;
            crossAdjacent = (sender as ToolStripMenuItem).Checked;
            updateParameters();
            Redraw();
        }

        private void crossCornerToolStripMenuItem_Click(object sender, EventArgs e) {
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;
            crossCorners = (sender as ToolStripMenuItem).Checked;
            updateParameters();
            Redraw();
        }

        //heurestic mode
        private void manhattanToolStripMenuItem_Click(object sender, EventArgs e) {
            if ((sender as ToolStripMenuItem).Checked)
                return;
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;
            mode = HeuristicMode.MANHATTAN;
            euclideanToolStripMenuItem.Checked = false;
            chebyshevToolStripMenuItem.Checked = false;
        }

        private void euclideanToolStripMenuItem_Click(object sender, EventArgs e) {
            if ((sender as ToolStripMenuItem).Checked)
                return;
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;
            mode = HeuristicMode.EUCLIDEAN;
            manhattanToolStripMenuItem.Checked = false;
            chebyshevToolStripMenuItem.Checked = false;
        }

        private void chebyshevToolStripMenuItem_Click(object sender, EventArgs e) {
            if ((sender as ToolStripMenuItem).Checked)
                return;
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;
            mode = HeuristicMode.CHEBYSHEV;
            manhattanToolStripMenuItem.Checked = false;
            euclideanToolStripMenuItem.Checked = false;
        }

        private void stepsToolStripMenuItem_Click(object sender, EventArgs e) {
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;

            if (sender as ToolStripMenuItem == bordersToolStripMenuItem)
                updateBorderVisibility(!bordersToolStripMenuItem.Checked);
            else if (sender as ToolStripMenuItem == highlightOverCurrentBoxToolStripMenuItem)
                allowHighlight = highlightOverCurrentBoxToolStripMenuItem.Checked;

            Redraw();
            Invalidate();

        }

        private void borderColorToolStripMenuItem_Click(object sender, EventArgs e) {
            if (cd_grid.ShowDialog() == DialogResult.OK) {
                BackColor = cd_grid.Color;
                selectedColor = cd_grid.Color;
                borderColorToolStripMenuItem.Checked = true;
            }
        }

        private void wallsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (nUD_AGVs.Value != 0)
                for (int agv = 0; agv < nUD_AGVs.Value; agv++)
                    AGVs[agv].JumpPoints.Clear();
            
            for (int widthTrav = 0; widthTrav < Constants.__WidthBlocks; widthTrav++)
                for (int heightTrav = 0; heightTrav < Constants.__HeightBlocks; heightTrav++)
                    switch (m_rectangles[widthTrav][heightTrav].boxType) {
                        case BoxType.Normal:
                        case BoxType.Start:
                        case BoxType.End:
                            break;
                        case BoxType.Wall:
                            m_rectangles[widthTrav][heightTrav].SetNormalBox();
                            break;
                    }
            Invalidate();
            Redraw();
        }

        private void allToolStripMenuItem_Click(object sender, EventArgs e) {

            FullyRestore();
        }

        private void exportMapToolStripMenuItem_Click(object sender, EventArgs e) {
            export();
        }
      
        private void importMapToolStripMenuItem_Click(object sender, EventArgs e) {
            import();
        }
        
        private void startToolStripMenuItem_Click(object sender, EventArgs e) {
            //start the animations

            //refresh the numeric value regarding the drawn agvs
            nUD_AGVs.Value = getNumberOfAGVs();

            //if we add more than 2 agvs,we have to resize the monitor.
            if (nUD_AGVs.Value > 2)
                gb_monitor.Width += gb_monitor.Width - 100;


            for (int i = 0; i < fromstart.Length; i++)
                fromstart[i] = true;

            beforeStart = false;
            allowHighlight = false;//do not allow highlight while emulation is active

            for (int i = 0; i < StartPos.Count; i++)
                AGVs[i].MarkedLoad = new Point();

            Redraw();

            for (int i = 0; i < StartPos.Count; i++) {
                AGVs[i].StartX = m_rectangles[StartPos[i].x][StartPos[i].y].boxRec.X;
                AGVs[i].StartY = m_rectangles[StartPos[i].x][StartPos[i].y].boxRec.Y;
                AGVs[i].SizeX = Constants.__BlockSide - 1;
                AGVs[i].SizeY = Constants.__BlockSide - 1;
                AGVs[i].Init();
            }
            
            timer_counter = new int[StartPos.Count];
            timers();
            settings_menu.Enabled = false;
            gb_settings.Enabled = false;

            show_emissions();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            About about = new About();
            about.ShowDialog();
        }

        private void increaseSpeedToolStripMenuItem_Click(object sender, EventArgs e) {
            int d = timer0.Interval;
            d += 100;
            timer0.Interval = timer1.Interval = timer2.Interval = timer3.Interval = timer4.Interval =d;
            refresh_label.Text = "Delay:" + timer0.Interval + " ms";
        }

        private void decreaseSpeedToolStripMenuItem_Click(object sender, EventArgs e) {
            if (timer0.Interval == 100)
                return;

            int d = timer0.Interval;
            d -= 100;
            timer0.Interval = timer1.Interval = timer2.Interval = timer3.Interval = timer4.Interval = d;
            refresh_label.Text = "Delay:" + timer0.Interval + " ms";

        }

       

        private void borderColorToolStripMenuItem1_Click(object sender, EventArgs e) {
            BackColor = Color.DarkGray;
            borderColorToolStripMenuItem.Checked = false;
        }

        private void resolutionToolStripMenuItem_Click(object sender, EventArgs e) {
            resolution res = new resolution();
            if (res.ShowDialog() == DialogResult.OK)
                FullyRestore();
            
        }
        
        private void main_form_LocationChanged(object sender, EventArgs e) {

            emissions.Location = new Point(Location.X + Size.Width, Location.Y);

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e) {

            int c = 0;
            for (int i = 0; i < StartPos.Count; i++)
                c += AGVs[i].JumpPoints.Count;

            if (c > 0)
                triggerStartMenu(true);
            else
                triggerStartMenu(false);
        }


        //one timer for each agv.
        private void timer0_Tick(object sender, EventArgs e) {
            int mysteps = 0;//init the steps
            for (int i = 0; i < Constants.__MaximumSteps; i++)
                if (AGVs[0].Steps[i].X == 0 || AGVs[0].Steps[i].Y == 0)
                    i = Constants.__MaximumSteps;
                else
                    mysteps++;//really count the steps
            
            AGVs[0].StepsCounter = mysteps;//add them inside the class

            animator(timer_counter[0], 0); //animate that class/agv

            timer_counter[0]++;
        }
        private void timer1_Tick(object sender, EventArgs e) {
            int mysteps = 0;
            for (int i = 0; i < Constants.__MaximumSteps; i++)
                if (AGVs[1].Steps[i].X == 0 || AGVs[1].Steps[i].Y == 0)
                    i = Constants.__MaximumSteps;
                else
                    mysteps++;
            
            AGVs[1].StepsCounter = mysteps;

            animator(timer_counter[1], 1);

            timer_counter[1]++;
        }

        private void timer2_Tick(object sender, EventArgs e) {
            int mysteps = 0;
            for (int i = 0; i < Constants.__MaximumSteps; i++)
                if (AGVs[2].Steps[i].X == 0 || AGVs[2].Steps[i].Y == 0)
                    i = Constants.__MaximumSteps;
                else
                    mysteps++;
            
            AGVs[2].StepsCounter = mysteps;

            animator(timer_counter[2], 2);

            timer_counter[2]++;
        }

        private void timer3_Tick(object sender, EventArgs e) {
            int mysteps = 0;
            for (int i = 0; i < Constants.__MaximumSteps; i++)
                if (AGVs[3].Steps[i].X == 0 || AGVs[3].Steps[i].Y == 0)
                    i = Constants.__MaximumSteps;
                else
                    mysteps++;
            
            AGVs[3].StepsCounter = mysteps;

            animator(timer_counter[3], 3);

            timer_counter[3]++;
        }

        private void timer4_Tick(object sender, EventArgs e) {
            int mysteps = 0;
            for (int i = 0; i < Constants.__MaximumSteps; i++)
                if (AGVs[4].Steps[i].X == 0 || AGVs[4].Steps[i].Y == 0)
                    i = Constants.__MaximumSteps;
                else
                    mysteps++;
            
            AGVs[4].StepsCounter = mysteps;

            animator(timer_counter[4], 4);

            timer_counter[4]++;
        }

        private void importImageLayoutToolStripMenuItem_Click(object sender, EventArgs e) {
            importImage();
        }

        private void priorityRulesbetaToolStripMenuItem_Click(object sender, EventArgs e) {
            use_Halt = !use_Halt;
            if (use_Halt)
                priorityRulesbetaToolStripMenuItem.Checked = true;
            else
                priorityRulesbetaToolStripMenuItem.Checked = false;
        }
    }

}
