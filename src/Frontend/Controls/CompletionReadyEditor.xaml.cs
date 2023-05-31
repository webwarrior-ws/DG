using System.Collections.ObjectModel;
using System.Text;

namespace Frontend.Controls;

public partial class CompletionReadyEditor : StackLayout
{
    #region Properties

    public static readonly BindableProperty AutocompletedWordsProperty =
        BindableProperty.Create(nameof(AutocompletedWords), typeof(List<string>), typeof(CompletionReadyEditor), new List<string>(), BindingMode.TwoWay);

    public static readonly BindableProperty TextProperty =
       BindableProperty.Create(nameof(AutocompletedWords), typeof(string), typeof(CompletionReadyEditor), string.Empty, BindingMode.TwoWay);

    public List<string> AutocompletedWords {
        set => SetValue(AutocompletedWordsProperty, value);
        private get => (List<string>)GetValue(AutocompletedWordsProperty);
    }

    public string Text {
        set {
            addedWords.Clear();
            value.Split(' ').Where(x => x.Length > 1).ToList().ForEach(x => addedWords.Add(x.ToLower()));
            SetValue(TextProperty, value);
        }
        get => (string)GetValue(TextProperty);
    }

    public event EventHandler EditorFocused;

    public bool CanSelectAll = true;

    #endregion

    #region Private fields

    private List<string> filteredOptions;
    private HashSet<string> addedWords = new HashSet<string>();
    private string latestWord;

    #endregion

    public CompletionReadyEditor()
    {
        InitializeComponent();
    }

    #region Events

    void MainEditorTextChanged(object sender, TextChangedEventArgs eventArgs)
    {
        string textInEditor = Text = MainEditor.Text.ToLower();
        latestWord = textInEditor.Contains(' ') ? textInEditor.Substring(textInEditor.LastIndexOf(' ') + 1).Trim() : textInEditor.Trim();
        filteredOptions = new List<string>(AutocompletedWords.Where(x => x.StartsWith(latestWord) && !addedWords.Contains(x)));
        MultiplePickerList.IsVisible = latestWord.Length > 0;
        BindableLayout.SetItemsSource(MultiplePickerList, filteredOptions);
    }

    void AddNewWordClicked(object sender, EventArgs eventArgs)
    {
        AutocompletedWords.Add(latestWord.ToString().ToLower());
        MainEditor.Text += " ";
    }

    void SelectedWordClicked(object sender, TappedEventArgs tappedEventArgs)
    {
        string selectedWord = ((Label)(sender)).Text;
        string textInEditor = MainEditor.Text;

        if (textInEditor.Contains(' '))
            MainEditor.Text = textInEditor.Substring(0, textInEditor.LastIndexOf(' ')) + ' ' + selectedWord + ' ';
        else MainEditor.Text = selectedWord + ' ';
        MainEditor.CursorPosition = MainEditor.Text.Length;
    }

    void MainEditorFocused(object sender, FocusEventArgs eventArgs)
    {
        EditorFocused?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Public Methods

    public void SelectAllText()
    {
        MainEditor.CursorPosition = 0;
        MainEditor.SelectionLength = MainEditor.Text.Length;
    }

    #endregion

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == TextProperty.PropertyName)
            MainEditor.Text = Text;
    }
}