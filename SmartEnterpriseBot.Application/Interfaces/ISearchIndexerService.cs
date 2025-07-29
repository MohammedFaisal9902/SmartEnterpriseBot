using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnterpriseBot.Application.Interfaces
{
    public interface ISearchIndexerService
    {
        Task<bool> UploadDocumentAsync(string content, List<string> allowedRoles);
    }
}
