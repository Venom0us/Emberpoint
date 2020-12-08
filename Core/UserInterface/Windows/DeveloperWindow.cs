﻿using Emberpoint.Core.Extensions;
using Emberpoint.Core.GameObjects.Interfaces;
using Emberpoint.Core.GameObjects.Managers;
using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SadConsole.Input;
using SadConsole.Themes;
using System.Collections.Generic;
using System.Linq;

namespace Emberpoint.Core.UserInterface.Windows
{
    public class DeveloperWindow : ControlsConsole, IUserInterface
    {
        public Console Console => this;
        private readonly Console _textConsole;
        private readonly TextBox _textInput;
        private readonly List<string> _previousLines;
        private readonly int _maxLineRows;

        public DeveloperWindow(int width, int height) : base(width, height)
        {
            // Set custom theme
            var colors = Colors.CreateDefault();
            colors.ControlBack = Color.Lerp(Color.Black, Color.Transparent, 0.6f);
            colors.Text = Color.White;
            colors.TitleText = Color.White;
            colors.ControlHostBack = Color.White;
            colors.RebuildAppearances();

            // Set the new theme colors         
            ThemeColors = colors;

            // Add text area
            _previousLines = new List<string>();
            _textConsole = new Console(width - 2, height - 3)
            {
                Position = new Point(1, 1)
            };

            _maxLineRows = _textConsole.Height -1;

            // Disable mouse, or it will steal mouse input from DeveloperConsole
            _textConsole.UseMouse = false;

            // Add input area
            _textInput = new TextBox(width - 2)
            {
                Position = _textConsole.Position + new Point(0, height - 3)
            };

            Add(_textInput);
            Children.Add(_textConsole);

            FocusedMode = ActiveBehavior.Push;

            // Middle of screen at the top
            Position = new Point(4, 2);

            Global.CurrentScreen.Children.Add(this);
        }

        public void Show()
        {
            IsVisible = true;
            IsFocused = true;
            Game.Player.IsFocused = false;
        }

        public void Hide()
        {
            IsVisible = false;
            IsFocused = false;
            Game.Player.IsFocused = true;
        }

        private void WriteText(string text, Color color)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            _textConsole.Clear();
            _textConsole.Cursor.Position = new Point(0, 0);

            if (_previousLines.Count == _maxLineRows)
            {
                _previousLines.RemoveAt(0);
            }

            _previousLines.Add(text);

            foreach (var line in _previousLines.Take(_maxLineRows))
            {
                _textConsole.Cursor.Print(new ColoredString(line, color, Color.Transparent));
                _textConsole.Cursor.CarriageReturn();
                _textConsole.Cursor.LineFeed();
            }
        }

        public override bool ProcessKeyboard(Keyboard info)
        {
            var baseValue = base.ProcessKeyboard(info);

            if (_textInput.DisableKeyboard && info.IsKeyPressed(KeybindingsManager.GetKeybinding(Keybindings.DeveloperConsole)))
            {
                if (IsVisible)
                    Hide();
                else
                    Show();
                return true;
            }

            // Check for enter key press
            for (int i=0; i < info.KeysPressed.Count; i++)
            {
                if (info.KeysPressed[i].Key == Microsoft.Xna.Framework.Input.Keys.Enter)
                {
                    if (!ParseCommand(_textInput.Text, out string output))
                        WriteText(_textInput.Text, Color.Red);
                    else
                        WriteText(output, Color.Green);
                    _textInput.Text = string.Empty;
                    return true;
                }
            }

            return baseValue;
        }

        public override bool ProcessMouse(MouseConsoleState state)
        {
            if (!_textInput.DisableKeyboard && state.Mouse.LeftClicked && !_textInput.MouseBounds.Contains(state.CellPosition))
            {
                _textInput.FocusLost();
                return true;
            }
            
            return base.ProcessMouse(state);
        }

        protected override void OnInvalidate()
        {
            base.OnInvalidate();

            // Draw borders for the controls console
            this.DrawBorders(Width, Height, "O", "|", "-", Color.Gray);
            Print(((Width / 2) - "Developer Console".Length / 2), 0, "Developer Console", Color.Orange);
        }

        public bool ParseCommand(string text, out string output)
        {
            output = "";
            return false;
        }

        public void ClearConsole()
        {
            _previousLines.Clear();
        }
    }
}
