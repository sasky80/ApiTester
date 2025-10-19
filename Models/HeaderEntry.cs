namespace ApiTester.Models
{
    using ReactiveUI;

    public class HeaderEntry : ReactiveObject
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string _value = string.Empty;
        public string Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        private bool _isFixed = false;
        public bool IsFixed
        {
            get => _isFixed;
            set => this.RaiseAndSetIfChanged(ref _isFixed, value);
        }
    }
}
