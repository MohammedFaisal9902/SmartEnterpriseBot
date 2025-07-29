using SmartEnterpriseBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnterpriseBot.Application.Interfaces
{
    /// <summary>
    /// Interface for AI-powered answer service that handles both structured and unstructured knowledge retrieval
    /// </summary>
    public interface IAiAnswerService
    {
        /// <summary>
        /// Gets an answer to a question by first checking structured knowledge base, 
        /// then falling back to unstructured documents using RAG (Retrieval-Augmented Generation)
        /// </summary>
        /// <param name="question">The question to answer</param>
        /// <param name="userRole">The role of the user making the request (for access control)</param>
        /// <param name="userId">The ID of the user making the request (for logging and monitoring)</param>
        /// <returns>The answer string, or null if no answer could be generated</returns>
        Task<string?> GetAnswerAsync(string question, Role userRole, string userId);
    }
}
