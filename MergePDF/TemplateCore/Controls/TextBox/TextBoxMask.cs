//-----------------------------------------------------------------------
// <copyright file="TextBoxMask.cs" company="Lifeprojects.de">
//     Class: TextBoxMask
//     Copyright © Lifeprojects.de 2026
// </copyright>
//
// <author>2026 - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>15.08.2026</date>
//
// <summary>
// Die Klasse stellt einen TextBox zur Verfügung, bei der Eingabe über eine Maske erfolgt.
// </summary>
// <example>
// <local:TextBoxMask x: Name = "PhoneTextBox" Width = "250" Mask = "(000) 000 000 000" />
// Zeichen	Bedeutung
// 0		Ziffer, erforderlich
// 9		Ziffer oder Leerzeichen, optional
// #		Ziffer oder Leerzeichen, erforderlich
// L		Buchstabe, erforderlich
// ?		Buchstabe, optional
// &		beliebiges Zeichen, erforderlich
// C		beliebiges Zeichen, optional
// A		alphanumerisch, erforderlich
// a		alphanumerisch, optional
// _ 		beliebiges Zeichen, erforderlich
// 			Leerzeichen
// .		Dezimaltrenner
// , 		Tausendertrenner
// :		Zeittrenner
// / 		Datentrenner
// $		Währungssymbol
// < 		nachfolgend Kleinbuchstaben
// > 		nachfolgend Großbuchstaben
// \		nächstes Zeichen als Literal
//</example>
//-----------------------------------------------------------------------

namespace System.Windows.Controls
{
    using System.Text;
    using System.Windows.Input;

    public class TextBoxMask : TextBox
    {
        #region Dependency Properties

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register(
                nameof(Mask),
                typeof(string),
                typeof(TextBoxMask),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnMaskChanged));

        public string Mask
        {
            get => (string)GetValue(MaskProperty);
            set => SetValue(MaskProperty, value);
        }

        public static readonly DependencyProperty RawTextProperty =
            DependencyProperty.Register(
                nameof(RawText),
                typeof(string),
                typeof(TextBoxMask),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnRawTextChanged));

        /// <summary>
        /// Der Inhalt ohne Maskenliterale.
        /// </summary>
        public string RawText
        {
            get => (string)GetValue(RawTextProperty) ?? string.Empty;
            set => SetValue(RawTextProperty, value ?? string.Empty);
        }

        /// <summary>
        /// Alias für RawText.
        /// </summary>
        public string Value
        {
            get => RawText;
            set => RawText = value;
        }

        #endregion Dependency Properties

        #region Fields

        private readonly List<TextBoxMaskPart> _parts = new();

        private bool _internalUpdate;
        private bool _isUndoRedo;

        private readonly Stack<TextBoxEditState> _undoStack = new();
        private readonly Stack<TextBoxEditState> _redoStack = new();

        private const int MaxUndoSteps = 100;

        #endregion

        #region Constructor

        public TextBoxMask()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, this.OnNewPaste));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, this.OnNewCopy));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, this.OnNewCut));
            this.Height = 22;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
            this.Padding = new Thickness(0, 0, 0, 0);

            Loaded += (_, _) =>
            {
                this.ParseMask();
                this.NormalizeRawText();
                this.RefreshText();
            };
        }

        #endregion

        #region Mask Parsing

        private void ParseMask()
        {
            _parts.Clear();

            if (string.IsNullOrEmpty(Mask))
                return;

            bool lowerCase = false;
            bool upperCase = false;
            bool escape = false;

            foreach (char c in Mask)
            {
                if (escape)
                {
                    _parts.Add(TextBoxMaskPart.Literal(c));
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '<')
                {
                    lowerCase = true;
                    upperCase = false;
                    continue;
                }

                if (c == '>')
                {
                    upperCase = true;
                    lowerCase = false;
                    continue;
                }

                if (c == '|')
                {
                    lowerCase = false;
                    upperCase = false;
                    continue;
                }

                if (IsPlaceholder(c))
                {
                    _parts.Add(
                        TextBoxMaskPart.Placeholder(
                            c,
                            lowerCase,
                            upperCase));
                }
                else
                {
                    _parts.Add(TextBoxMaskPart.Literal(c));
                }
            }
        }

        private static bool IsPlaceholder(char c)
        {
            return c switch
            {
                '0' => true,
                '9' => true,
                '#' => true,
                'L' => true,
                '?' => true,
                '&' => true,
                'C' => true,
                'A' => true,
                'a' => true,

                // Benutzerdefinierter Platzhalter
                '_' => true,

                _ => false
            };
        }

        private static bool IsOptional(char c)
        {
            return c switch
            {
                '9' => true,
                '?' => true,
                'C' => true,
                'a' => true,
                _ => false
            };
        }

        #endregion

        #region Character Validation

        private static bool Accepts(char mask, char value)
        {
            return mask switch
            {
                '0' => char.IsDigit(value),

                '9' =>
                    char.IsDigit(value) ||
                    value == ' ',

                '#' =>
                    char.IsDigit(value) ||
                    value == ' ',

                'L' =>
                    char.IsLetter(value),

                '?' =>
                    char.IsLetter(value),

                '&' =>
                    true,

                'C' =>
                    true,

                'A' =>
                    char.IsLetterOrDigit(value),

                'a' =>
                    char.IsLetterOrDigit(value),

                '_' =>
                    true,

                _ =>
                    false
            };
        }

        private static char ConvertCharacter(char value, bool lowerCase, bool upperCase)
        {
            if (lowerCase)
            {
                return char.ToLowerInvariant(value);
            }

            if (upperCase)
            {
                return char.ToUpperInvariant(value);
            }

            return value;
        }

        #endregion

        #region Rendering

        private void RefreshText()
        {
            if (this._internalUpdate)
            {
                return;
            }

            this.ParseMask();

            string oldText = Text;

            int rawCaret = this.CaretIndexToRawIndex(this.CaretIndex);

            string formatted = this.FormatRawText(this.RawText);

            this._internalUpdate = true;

            try
            {
                this.Text = formatted;

                int newCaret = this.RawIndexToCaretIndex(Math.Min(rawCaret, this.RawText == null ? 0 : this.RawText.Length));

                this.CaretIndex = Math.Min( newCaret, this.Text.Length);

                this.SelectionLength = 0;
            }
            finally
            {
                this._internalUpdate = false;
            }
        }

        private string FormatRawText(string raw)
        {
            if (_parts.Count == 0)
            {
                return raw ?? string.Empty;
            }

            raw ??= string.Empty;

            var result = new StringBuilder();

            int rawIndex = 0;

            foreach (TextBoxMaskPart part in _parts)
            {
                if (part.IsLiteral)
                {
                    result.Append(part.Character);
                    continue;
                }

                if (rawIndex < raw.Length)
                {
                    char value = raw[rawIndex];

                    value = ConvertCharacter(value, part.LowerCase, part.UpperCase);

                    result.Append(value);

                    rawIndex++;
                }
                else
                {
                    // Leere Maskenposition anzeigen
                    result.Append('_');
                }
            }

            return result.ToString();
        }

        #endregion

        #region Keyboard Input

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (this._internalUpdate)
            {
                base.OnPreviewTextInput(e);
                return;
            }

            e.Handled = true;

            this.ReplaceSelectionWith(e.Text);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this._internalUpdate)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

            bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

            // Undo
            if (ctrl && e.Key == Key.Z)
            {
                if (shift == true)
                {
                    this.Redo();
                }
                else
                {
                    this.Undo();
                }

                e.Handled = true;
                return;
            }

            // Redo
            if (ctrl && e.Key == Key.Y)
            {
                this.Redo();

                e.Handled = true;
                return;
            }

            switch (e.Key)
            {
                case Key.Back:

                    if (this.SelectionLength > 0)
                    {
                        this.DeleteSelection();
                    }
                    else
                    {
                        this.Backspace();
                    }

                    e.Handled = true;
                    return;

                case Key.Delete:

                    if (this.SelectionLength > 0)
                    {
                        this.DeleteSelection();
                    }
                    else
                    {
                        this.Delete();
                    }

                    e.Handled = true;
                    return;

                case Key.Left:

                    this.MoveCaretLeft(shift);

                    e.Handled = true;
                    return;

                case Key.Right:

                    this.MoveCaretRight(shift);

                    e.Handled = true;
                    return;

                case Key.Home:

                    if (shift == true)
                    {
                        int old = CaretIndex;
                        this.CaretIndex = 0;
                        this.SelectFrom(old);
                    }
                    else
                    {
                        this.CaretIndex = 0;
                        this.SelectionLength = 0;
                    }

                    e.Handled = true;
                    return;

                case Key.End:

                    if (shift == true)
                    {
                        int old = this.CaretIndex;
                        this.CaretIndex = Text.Length;
                        this.SelectFrom(old);
                    }
                    else
                    {
                        this.CaretIndex = this.Text.Length;
                        this.SelectionLength = 0;
                    }

                    e.Handled = true;
                    return;
            }

            base.OnPreviewKeyDown(e);
        }

        #endregion

        #region Paste / Cut / Copy

        private void OnNewPaste(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Clipboard.ContainsText())
                return;

            string clipboardText = Clipboard.GetText();

            ReplaceSelectionWith(clipboardText);

            e.Handled = true;
        }

        private void OnNewCopy(object sender, ExecutedRoutedEventArgs e)
        {
            if (SelectionLength <= 0)
            {
                return;
            }

            string rawText = this.GetRawTextFromSelection();

            if (string.IsNullOrEmpty(rawText) == false)
            {
                Clipboard.SetText(rawText);
            }

            e.Handled = true;
        }

        private void OnNewCut(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.SelectionLength <= 0)
            {
                return;
            }

            string rawText = this.GetRawTextFromSelection();

            if (string.IsNullOrEmpty(rawText) == false)
            {
                Clipboard.SetText(rawText);
            }

            this.DeleteSelection();

            e.Handled = true;
        }

        #endregion

        #region Insert

        private void ReplaceSelectionWith(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return;
            }

            this.SaveUndoState();

            int startRaw = this.CaretIndexToRawIndex(this.SelectionStart);

            int endRaw = this.CaretIndexToRawIndex(this.SelectionStart + this.SelectionLength);

            string current = this.RawText ?? string.Empty;

            if (endRaw > startRaw)
            {
                int removeLength = Math.Min(endRaw - startRaw, current.Length - startRaw);

                if (removeLength > 0)
                {
                    current = current.Remove(startRaw, removeLength);
                }
            }

            int rawIndex = startRaw;

            foreach (char inputCharacter in input)
            {
                int maskIndex = this.GetNextEditableMaskIndex(rawIndex);

                if (maskIndex < 0)
                {
                    break;
                }

                TextBoxMaskPart part = this._parts[maskIndex];

                if (Accepts(part.Character, inputCharacter) == false)
                {
                    continue;
                }

                char value = ConvertCharacter(inputCharacter, part.LowerCase, part.UpperCase);

                if (rawIndex < current.Length)
                {
                    current = current.Remove(rawIndex, 1);
                    current = current.Insert(rawIndex, value.ToString());
                }
                else
                {
                    current += value;
                }

                rawIndex++;
            }

            this.SetRawTextInternal(current);
            this.RefreshText();

            CaretIndex =
                RawIndexToCaretIndex(rawIndex);

            SelectionLength = 0;
        }

        #endregion

        #region Delete

        private void Backspace()
        {
            int rawIndex = this.CaretIndexToRawIndex(CaretIndex);

            if (rawIndex <= 0)
            {
                return;
            }

            this.SaveUndoState();

            rawIndex--;

            string value = this.RawText;

            if (rawIndex < value.Length)
            {
                value = value.Remove(rawIndex, 1);
            }

            this.SetRawTextInternal(value);

            this.RefreshText();

            this.CaretIndex = this.RawIndexToCaretIndex(rawIndex);
        }

        private void Delete()
        {
            int rawIndex = this.CaretIndexToRawIndex(this.CaretIndex);

            if (rawIndex >= this.RawText.Length)
            {
                return;
            }

            this.SaveUndoState();

            string value = this.RawText;

            value = value.Remove(rawIndex, 1);

            this.SetRawTextInternal(value);

            this.RefreshText();

            this.CaretIndex = this.RawIndexToCaretIndex(rawIndex);
        }

        private void DeleteSelection()
        {
            int startRaw = this.CaretIndexToRawIndex(this.SelectionStart);

            int endRaw = this.CaretIndexToRawIndex(this.SelectionStart + this.SelectionLength);

            if (endRaw <= startRaw)
            {
                return;
            }

            this.SaveUndoState();

            string value = this.RawText.Remove(startRaw, endRaw - startRaw);

            this.SetRawTextInternal(value);

            this.RefreshText();

            this.CaretIndex =  this.RawIndexToCaretIndex(startRaw);

            this.SelectionLength = 0;
        }

        #endregion

        #region Cursor

        private void MoveCaretLeft(bool shift)
        {
            int oldCaret = CaretIndex;

            if (CaretIndex <= 0)
            {
                return;
            }

            int raw = this.CaretIndexToRawIndex(this.CaretIndex);

            raw = Math.Max(0, raw - 1);

            CaretIndex = this.RawIndexToCaretIndex(raw);

            if (shift == true)
            {
                this.SelectFrom(oldCaret);
            }
            else
            {
                this.SelectionLength = 0;
            }
        }

        private void MoveCaretRight(bool shift)
        {
            int oldCaret = CaretIndex;

            int raw = this.CaretIndexToRawIndex(this.CaretIndex);

            if (raw >= RawText.Length)
            {
                return;
            }

            raw++;

            this.CaretIndex = this.RawIndexToCaretIndex(raw);

            if (shift == true)
            {
                this.SelectFrom(oldCaret);
            }
            else
            {
                this.SelectionLength = 0;
            }
        }

        private void SelectFrom(int anchor)
        {
            int current = CaretIndex;

            if (current >= anchor)
            {
                this.SelectionStart = anchor;
                this.SelectionLength = current - anchor;
            }
            else
            {
                this.SelectionStart = current;
                this.SelectionLength = anchor - current;
            }
        }

        #endregion

        #region Raw / Visual Mapping

        private int CaretIndexToRawIndex(int caret)
        {
            if (_parts.Count == 0)
            {
                return Math.Min(caret, RawText.Length);
            }

            int visual = 0;
            int raw = 0;

            foreach (TextBoxMaskPart part in _parts)
            {
                if (visual >= caret)
                {
                    break;
                }

                visual++;

                if (!part.IsLiteral)
                {
                    if (raw < RawText.Length)
                    {
                        raw++;
                    }
                }
            }

            return Math.Min(raw, RawText == null ? 0 :RawText.Length);
        }

        private int RawIndexToCaretIndex(int rawIndex)
        {
            if (_parts.Count == 0)
                return Math.Min(rawIndex, RawText.Length);

            int visual = 0;
            int raw = 0;

            foreach (TextBoxMaskPart part in _parts)
            {
                if (part.IsLiteral)
                {
                    /*
                     * Literale werden automatisch übersprungen,
                     * wenn der Cursor davor landet und noch Inhalt
                     * folgt.
                     */
                    if (raw < rawIndex)
                    {
                        visual++;
                    }

                    continue;
                }

                if (raw >= rawIndex)
                {
                    break;
                }

                raw++;
                visual++;
            }

            return visual;
        }

        private int GetNextEditableMaskIndex(int rawIndex)
        {
            int currentRaw = 0;

            for (int i = 0; i < _parts.Count; i++)
            {
                if (_parts[i].IsLiteral == true)
                {
                    continue;
                }

                if (currentRaw == rawIndex)
                {
                    return i;
                }

                currentRaw++;
            }

            return -1;
        }

        #endregion

        #region Selection / Copy

        private string GetRawTextFromSelection()
        {
            if (SelectionLength <= 0)
            {
                return string.Empty;
            }

            int startRaw = this.CaretIndexToRawIndex(SelectionStart);

            int endRaw = this.CaretIndexToRawIndex(SelectionStart + SelectionLength);

            if (endRaw <= startRaw)
            {
                return string.Empty;
            }

            return RawText.Substring(startRaw, endRaw - startRaw);
        }

        #endregion Selection / Copy

        #region Undo / Redo

        private void SaveUndoState()
        {
            if (this._isUndoRedo == true)
            {
                return;
            }

            this._undoStack.Push(
                new TextBoxEditState(
                    RawText,
                    CaretIndex,
                    SelectionStart,
                    SelectionLength));

            while (this._undoStack.Count > MaxUndoSteps)
            {
                RemoveOldest(this._undoStack);
            }

            this._redoStack.Clear();
        }

        new private void Undo()
        {
            if (this._undoStack.Count == 0)
            {
                return;
            }

            TextBoxEditState current =
                new(
                    RawText,
                    CaretIndex,
                    SelectionStart,
                    SelectionLength);

            TextBoxEditState previous = this._undoStack.Pop();

            this._redoStack.Push(current);

            this.RestoreState(previous);
        }

        new private void Redo()
        {
            if (_redoStack.Count == 0)
            {
                return;
            }

            TextBoxEditState current =
                new(
                    RawText,
                    CaretIndex,
                    SelectionStart,
                    SelectionLength);

            TextBoxEditState next = _redoStack.Pop();

            this._undoStack.Push(current);

            this.RestoreState(next);
        }

        private void RestoreState(TextBoxEditState state)
        {
            this._isUndoRedo = true;

            try
            {
                this.SetRawTextInternal(state.RawText);

                this.RefreshText();

                this.CaretIndex = Math.Min(state.CaretIndex, Text.Length);

                this.SelectionStart = Math.Min(state.SelectionStart, Text.Length);

                this.SelectionLength = Math.Min(state.SelectionLength, Text.Length - SelectionStart);
            }
            finally
            {
                this._isUndoRedo = false;
            }
        }

        private static void RemoveOldest(Stack<TextBoxEditState> stack)
        {
            if (stack.Count == 0)
            {
                return;
            }

            TextBoxEditState[] states = stack.ToArray();

            stack.Clear();

            for (int i = states.Length - 2; i >= 0; i--)
            {
                stack.Push(states[i]);
            }
        }

        #endregion

        #region RawText

        private void SetRawTextInternal(string value)
        {
            this._internalUpdate = true;

            try
            {
                this.SetCurrentValue(RawTextProperty, value ?? string.Empty);
            }
            finally
            {
                this._internalUpdate = false;
            }
        }

        private static void OnRawTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBoxMask control)
            {
                return;
            }

            if (control._internalUpdate)
            {
                return;
            }

            control.RefreshText();
        }

        private static void OnMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBoxMask control)
            {
                return;
            }

            control.ParseMask();

            /*
             * Wenn sich die Maske ändert, muss der bisherige
             * RawText-Wert gegen die neue Maske geprüft werden.
             */
            control.NormalizeRawText();

            control.RefreshText();
        }

        #endregion

        #region Normalization

        private void NormalizeRawText()
        {
            if (string.IsNullOrEmpty(RawText) == true)
            {
                return;
            }

            string source = RawText;
            var result = new StringBuilder();

            int rawIndex = 0;

            foreach (char character in source)
            {
                int maskIndex = this.GetNextEditableMaskIndex(rawIndex);

                if (maskIndex < 0)
                {
                    break;
                }

                TextBoxMaskPart part = this._parts[maskIndex];

                if (Accepts(part.Character, character) == false)
                {
                    continue;
                }

                result.Append(ConvertCharacter(character, part.LowerCase, part.UpperCase));

                rawIndex++;
            }

            this.SetRawTextInternal(result.ToString());
        }

        #endregion

        #region Mask Validation

        /// <summary>
        /// Gibt zurück, ob der aktuelle Inhalt
        /// alle Pflichtfelder der Maske erfüllt.
        /// </summary>
        public bool IsMaskComplete
        {
            get
            {
                int rawIndex = 0;

                foreach (TextBoxMaskPart part in _parts)
                {
                    if (part.IsLiteral)
                        continue;

                    if (rawIndex >= RawText.Length)
                    {
                        if (!IsOptional(part.Character))
                            return false;

                        continue;
                    }

                    if (!Accepts(
                            part.Character,
                            RawText[rawIndex]))
                    {
                        return false;
                    }

                    rawIndex++;
                }

                return true;
            }
        }

        #endregion

        #region MaskPart

        private sealed class TextBoxMaskPart
        {
            public bool IsLiteral { get; private init; }

            public char Character { get; private init; }

            public bool LowerCase { get; private init; }

            public bool UpperCase { get; private init; }

            public static TextBoxMaskPart Literal(char character)
            {
                return new TextBoxMaskPart
                {
                    IsLiteral = true,
                    Character = character
                };
            }

            public static TextBoxMaskPart Placeholder(char character, bool lowerCase, bool upperCase)
            {
                return new TextBoxMaskPart
                {
                    IsLiteral = false,
                    Character = character,
                    LowerCase = lowerCase,
                    UpperCase = upperCase
                };
            }
        }

        #endregion

        #region EditState

        private sealed class TextBoxEditState
        {
            public string RawText { get; }

            public int CaretIndex { get; }

            public int SelectionStart { get; }

            public int SelectionLength { get; }

            public TextBoxEditState(
                string rawText,
                int caretIndex,
                int selectionStart,
                int selectionLength)
            {
                RawText = rawText;
                CaretIndex = caretIndex;
                SelectionStart = selectionStart;
                SelectionLength = selectionLength;
            }
        }

        #endregion
    }
}
