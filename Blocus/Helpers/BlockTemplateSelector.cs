using Blocus.ViewModels;
using Microsoft.Maui.Controls;

namespace Blocus.Helpers
{
    public class BlockTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate CheckboxTemplate { get; set; }
        public DataTemplate PageTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is BlockViewModel vm)
            {
                var type = vm.Type?.ToLowerInvariant() ?? string.Empty;
                return type switch
                {
                    "checkbox" => CheckboxTemplate,
                    "page" => PageTemplate,
                    // Добавь другие типы, если появятся (например, "heading", "divider")
                    _ => TextTemplate,
                };
            }
            // Если что-то не то прилетело — вернуть дефолтный шаблон
            return TextTemplate;
        }
    }
}
