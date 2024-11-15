using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Email account service
    /// </summary>
    public partial interface IEmailAccountService
    {
        /// <summary>
        /// Inserts an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        void InsertEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Updates an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        void UpdateEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Deletes an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        void DeleteEmailAccount(EmailAccount emailAccount);

        /// <summary>
        /// Gets an email account by identifier
        /// </summary>
        /// <param name="emailAccountId">The email account identifier</param>
        /// <returns>Email account</returns>
        EmailAccount GetEmailAccountById(int emailAccountId);

        /// <summary>
        /// Gets all email accounts
        /// </summary>
        /// <returns>Email accounts list</returns>
        IList<EmailAccount> GetAllEmailAccounts();



        #region Email Receivers

        /// <summary>
        /// Insert email receivers
        /// </summary>
        /// <param name="emailReceivers">email receivers</param>
        void InsertEmailReceiver(EmailReceivers emailReceivers);

        /// <summary>
        /// Update email receivers
        /// </summary>
        /// <param name="emailReceivers">email receivers</param>
        void UpdateEmailReceiver(EmailReceivers emailReceivers);

        /// <summary>
        /// delete email receivers
        /// </summary>
        /// <param name="emailReceivers"></param>
        void DeleteEmailReceiver(EmailReceivers emailReceivers);

        /// <summary>
        /// Get all email receivers
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        IPagedList<EmailReceivers> GetAllEmailReceivers(int pageIndex, int pageSize);

        /// <summary>
        /// Get all email receivers by permission
        /// </summary>
        /// <param name="emailReceiverPermission">email receiver permission</param>
        /// <returns></returns>
        IList<EmailReceivers> GetAllEmailReceivers(EmailReceiverPermission emailReceiverPermission);

        /// <summary>
        /// Get email receiver by id
        /// </summary>
        /// <param name="id"></param>
        EmailReceivers GetEmailReceiver(int id);


        #endregion
    }
}
