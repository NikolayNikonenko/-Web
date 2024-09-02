using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Pages.ModelMistake
{
    public class SomeDataModel: PageModel
    {
        private readonly ApplicationContext _modelContext;

        public List<ModelErrors> ErrModel { get; set; }

        public SomeDataModel(ApplicationContext dbModel)
        {
            _modelContext = dbModel;
        }
        public void OnGet()
        {
            ErrModel = _modelContext.modelErrors.AsNoTracking().ToList();
        }

    }
}
