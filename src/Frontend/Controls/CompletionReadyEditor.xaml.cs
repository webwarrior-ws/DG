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
        set {
            SetValue(AutocompletedWordsProperty, value);
            filteredOptions = new List<string>(AutocompletedWords);
            BindableLayout.SetItemsSource(MultiplePickerList, filteredOptions);
        }
        private get => (List<string>)GetValue(AutocompletedWordsProperty);
    }

    public string Text {
        set {
            if (addedWords.Count == 0 && !string.IsNullOrWhiteSpace(value))
                value.Split(' ').ToList().ForEach(x => addedWords.Add(x.ToLower()));
            SetValue(TextProperty, value);
        }
        get => (string)GetValue(TextProperty);
    }

    #endregion

    #region Private fields

    private List<string> filteredOptions = new List<string>();
    private Dictionary<string, List<string>> filteredOptionsHistory = new Dictionary<string, List<string>>();
    private HashSet<string> addedWords = new HashSet<string>();

    StringBuilder word = new StringBuilder();

    #endregion

    public CompletionReadyEditor()
    {
        InitializeComponent();
    }

    #region Events

    void MainEditor_TextChanged(object sender, TextChangedEventArgs eventArgs)
    {
        if (eventArgs.NewTextValue.Length - 1 < 0 || eventArgs.NewTextValue[eventArgs.NewTextValue.Length - 1] == ' ')
        {
            filteredOptions = new List<string>(AutocompletedWords.Where(x => !addedWords.Contains(x.ToLower())));
            word.Clear();
        }
        else
        {
            if (eventArgs.OldTextValue == null || eventArgs.NewTextValue.Length > eventArgs.OldTextValue?.Length)
                word.Append(eventArgs.NewTextValue[eventArgs.NewTextValue.Length - 1]);
            else if (word.Length > 0)
                word.Remove(word.Length - 1, 1);

            var searchedItem = word.ToString().ToLower();

            if (filteredOptionsHistory.ContainsKey(searchedItem))
                filteredOptions = filteredOptionsHistory[searchedItem];
            else
            {
                filteredOptions = filteredOptions.Where(x => !addedWords.Contains(x.ToLower()) && x.ToLower().StartsWith(searchedItem)).ToList();
                filteredOptionsHistory.Add(searchedItem, filteredOptions);
            }
        }

        Text = MainEditor.Text;
        MultiplePickerList.IsVisible = word.Length > 0;
        BindableLayout.SetItemsSource(MultiplePickerList, filteredOptions);
    }

    void AddNewWordClicked(object sender, EventArgs eventArgs)
    {
        string selectedWord = word.ToString().ToLower();
        AutocompletedWords.Add(selectedWord);
        ClearHistoryAndWord(selectedWord);
        MainEditor.Text += " ";
    }

    void SelectedWordClicked(object sender, TappedEventArgs tappedEventArgs)
    {
        string selectedWord = ((Label)(sender)).Text;
        MainEditor.Text += selectedWord.Substring(word.ToString().Length) + " ";
        ClearHistoryAndWord(selectedWord);
    }

    #endregion

    void ClearHistoryAndWord(string selectedWord)
    {
        addedWords.Add(selectedWord.ToLower());
        word.Clear();
        filteredOptionsHistory.Clear();
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == TextProperty.PropertyName)
            MainEditor.Text = Text;
    }
}