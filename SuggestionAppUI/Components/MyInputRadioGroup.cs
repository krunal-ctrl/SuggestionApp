using Microsoft.AspNetCore.Components.Forms;
namespace SuggestionAppUI.Components;

public class MyInputRadioGroup<TValue>: InputRadioGroup<TValue>
{
    private string _name;
    private string _fieldClass;

    protected override void OnParametersSet()
    {
        var fieldCalss = EditContext?.FieldCssClass(FieldIdentifier) ?? string.Empty;
        if(fieldCalss != _fieldClass || Name != _name)
        {
            _name = Name;
            _fieldClass = fieldCalss;
            base.OnParametersSet();
        }
    }
}
