using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Caching.Extensions;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Email account service
    /// </summary>
    public partial class EmailAccountService : IEmailAccountService
    {
        #region Fields

        private readonly ICacheKeyService _cacheKeyService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<EmailReceivers> _emailReceiversRepository;

        #endregion

        #region Ctor

        public EmailAccountService(ICacheKeyService cacheKeyService,
            IEventPublisher eventPublisher,
            IRepository<EmailAccount> emailAccountRepository,
            IRepository<EmailReceivers> emailReceiversRepository)
        {
            _cacheKeyService = cacheKeyService;
            _eventPublisher = eventPublisher;
            _emailAccountRepository = emailAccountRepository;
            _emailReceiversRepository = emailReceiversRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual void InsertEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            emailAccount.Email = CommonHelper.EnsureNotNull(emailAccount.Email);
            emailAccount.DisplayName = CommonHelper.EnsureNotNull(emailAccount.DisplayName);
            emailAccount.Host = CommonHelper.EnsureNotNull(emailAccount.Host);
            emailAccount.Username = CommonHelper.EnsureNotNull(emailAccount.Username);
            emailAccount.Password = CommonHelper.EnsureNotNull(emailAccount.Password);

            emailAccount.Email = emailAccount.Email.Trim();
            emailAccount.DisplayName = emailAccount.DisplayName.Trim();
            emailAccount.Host = emailAccount.Host.Trim();
            emailAccount.Username = emailAccount.Username.Trim();
            emailAccount.Password = emailAccount.Password.Trim();

            emailAccount.Email = CommonHelper.EnsureMaximumLength(emailAccount.Email, 255);
            emailAccount.DisplayName = CommonHelper.EnsureMaximumLength(emailAccount.DisplayName, 255);
            emailAccount.Host = CommonHelper.EnsureMaximumLength(emailAccount.Host, 255);
            emailAccount.Username = CommonHelper.EnsureMaximumLength(emailAccount.Username, 255);
            emailAccount.Password = CommonHelper.EnsureMaximumLength(emailAccount.Password, 255);

            _emailAccountRepository.Insert(emailAccount);

            //event notification
            _eventPublisher.EntityInserted(emailAccount);
        }

        /// <summary>
        /// Updates an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual void UpdateEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            emailAccount.Email = CommonHelper.EnsureNotNull(emailAccount.Email);
            emailAccount.DisplayName = CommonHelper.EnsureNotNull(emailAccount.DisplayName);
            emailAccount.Host = CommonHelper.EnsureNotNull(emailAccount.Host);
            emailAccount.Username = CommonHelper.EnsureNotNull(emailAccount.Username);
            emailAccount.Password = CommonHelper.EnsureNotNull(emailAccount.Password);

            emailAccount.Email = emailAccount.Email.Trim();
            emailAccount.DisplayName = emailAccount.DisplayName.Trim();
            emailAccount.Host = emailAccount.Host.Trim();
            emailAccount.Username = emailAccount.Username.Trim();
            emailAccount.Password = emailAccount.Password.Trim();

            emailAccount.Email = CommonHelper.EnsureMaximumLength(emailAccount.Email, 255);
            emailAccount.DisplayName = CommonHelper.EnsureMaximumLength(emailAccount.DisplayName, 255);
            emailAccount.Host = CommonHelper.EnsureMaximumLength(emailAccount.Host, 255);
            emailAccount.Username = CommonHelper.EnsureMaximumLength(emailAccount.Username, 255);
            emailAccount.Password = CommonHelper.EnsureMaximumLength(emailAccount.Password, 255);

            _emailAccountRepository.Update(emailAccount);

            //event notification
            _eventPublisher.EntityUpdated(emailAccount);
        }

        /// <summary>
        /// Deletes an email account
        /// </summary>
        /// <param name="emailAccount">Email account</param>
        public virtual void DeleteEmailAccount(EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            if (GetAllEmailAccounts().Count == 1)
                throw new NopException("You cannot delete this email account. At least one account is required.");

            _emailAccountRepository.Delete(emailAccount);

            //event notification
            _eventPublisher.EntityDeleted(emailAccount);
        }

        /// <summary>
        /// Gets an email account by identifier
        /// </summary>
        /// <param name="emailAccountId">The email account identifier</param>
        /// <returns>Email account</returns>
        public virtual EmailAccount GetEmailAccountById(int emailAccountId)
        {
            if (emailAccountId == 0)
                return null;

            return _emailAccountRepository.ToCachedGetById(emailAccountId);
        }

        /// <summary>
        /// Gets all email accounts
        /// </summary>
        /// <returns>Email accounts list</returns>
        public virtual IList<EmailAccount> GetAllEmailAccounts()
        {
            var query = from ea in _emailAccountRepository.Table
                        orderby ea.Id
                        select ea;

            var emailAccounts = query.ToCachedList(_cacheKeyService.PrepareKeyForDefaultCache(NopMessageDefaults.EmailAccountsAllCacheKey));

            return emailAccounts;
        }

        #endregion


        #region Email Receivers

        /// <summary>
        /// Insert email receivers
        /// </summary>
        /// <param name="emailReceivers">email receivers</param>
        public virtual void InsertEmailReceiver(EmailReceivers emailReceivers) {
            if(emailReceivers == null)
                throw new ArgumentNullException("EmailReceivers");

            _emailReceiversRepository.Insert(emailReceivers);
        }

        /// <summary>
        /// Update email receivers
        /// </summary>
        /// <param name="emailReceivers">email receivers</param>
        public virtual void UpdateEmailReceiver(EmailReceivers emailReceivers) {
            if(emailReceivers == null)
                throw new ArgumentNullException("EmailReceivers");

            _emailReceiversRepository.Update(emailReceivers);
        }

        /// <summary>
        /// delete email receivers
        /// </summary>
        /// <param name="emailReceivers"></param>
        public virtual void DeleteEmailReceiver(EmailReceivers emailReceivers) {
            if(emailReceivers == null)
                throw new ArgumentNullException("EmailReceivers");

            _emailReceiversRepository.Delete(emailReceivers);
        }

        /// <summary>
        /// Get email receiver by id
        /// </summary>
        /// <param name="id"></param>
        public virtual EmailReceivers GetEmailReceiver(int id) {
            return _emailReceiversRepository.GetById(id);
        }

        /// <summary>
        /// Get all email receivers
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        public virtual IPagedList<EmailReceivers> GetAllEmailReceivers(int pageIndex,int pageSize) {
            var query = from p in _emailReceiversRepository.Table
                        orderby p.Id descending
                        select p;

            return new PagedList<EmailReceivers>(query, pageIndex, pageSize);
        }
       
        /// <summary>
        /// Get all email receivers by permission
        /// </summary>
        /// <param name="emailReceiverPermission">email receiver permission</param>
        /// <returns></returns>
        public virtual IList<EmailReceivers> GetAllEmailReceivers(EmailReceiverPermission emailReceiverPermission) {
            var query = (from p in _emailReceiversRepository.Table
                        orderby p.Id descending
                        select p).ToList();

            var records = query.Where(x => x.EmailReceiverPemissions.Any(x => x == emailReceiverPermission));
            return records.ToList();
        }


        #endregion
    }
}