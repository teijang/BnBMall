﻿using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Seo;
using Nop.Data;
using Nop.Services.Caching.Extensions;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Seo;

namespace Nop.Services.Affiliates
{
    /// <summary>
    /// Affiliate service
    /// </summary>
    public partial class AffiliateService : IAffiliateService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Affiliate> _affiliateRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<AffiliateType> _affiliateTypeRepository;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly SeoSettings _seoSettings;

        #endregion

        #region Ctor

        public AffiliateService(IAddressService addressService,
            IEventPublisher eventPublisher,
            IRepository<Address> addressRepository,
            IRepository<Affiliate> affiliateRepository,
            IRepository<AffiliateType> affiliateTypeRepository,
            IRepository<Order> orderRepository,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            SeoSettings seoSettings)
        {
            _addressService = addressService;
            _eventPublisher = eventPublisher;
            _addressRepository = addressRepository;
            _affiliateRepository = affiliateRepository;
            _affiliateTypeRepository = affiliateTypeRepository;
            _orderRepository = orderRepository;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _seoSettings = seoSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an affiliate by affiliate identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <returns>Affiliate</returns>
        public virtual Affiliate GetAffiliateById(int affiliateId)
        {
            if (affiliateId == 0)
                return null;

            return _affiliateRepository.ToCachedGetById(affiliateId);
        }

        /// <summary>
        /// Gets an affiliate by friendly URL name
        /// </summary>
        /// <param name="friendlyUrlName">Friendly URL name</param>
        /// <returns>Affiliate</returns>
        public virtual Affiliate GetAffiliateByFriendlyUrlName(string friendlyUrlName)
        {
            if (string.IsNullOrWhiteSpace(friendlyUrlName))
                return null;

            var query = from a in _affiliateRepository.Table
                        orderby a.Id
                        where a.FriendlyUrlName == friendlyUrlName
                        select a;
            var affiliate = query.FirstOrDefault();
            return affiliate;
        }

        /// <summary>
        /// Marks affiliate as deleted 
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual void DeleteAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            affiliate.Deleted = true;
            UpdateAffiliate(affiliate);

            //event notification
            _eventPublisher.EntityDeleted(affiliate);
        }

        /// <summary>
        /// Gets all affiliates
        /// </summary>
        /// <param name="friendlyUrlName">Friendly URL name; null to load all records</param>
        /// <param name="firstName">First name; null to load all records</param>
        /// <param name="lastName">Last name; null to load all records</param>
        /// <param name="loadOnlyWithOrders">Value indicating whether to load affiliates only with orders placed (by affiliated customers)</param>
        /// <param name="ordersCreatedFromUtc">Orders created date from (UTC); null to load all records. It's used only with "loadOnlyWithOrders" parameter st to "true".</param>
        /// <param name="ordersCreatedToUtc">Orders created date to (UTC); null to load all records. It's used only with "loadOnlyWithOrders" parameter st to "true".</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Affiliates</returns>
        public virtual IPagedList<Affiliate> GetAllAffiliates(string friendlyUrlName = null,
            string firstName = null, string lastName = null,
            bool loadOnlyWithOrders = false,
            DateTime? ordersCreatedFromUtc = null, DateTime? ordersCreatedToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            var query = _affiliateRepository.Table;

            if (!string.IsNullOrWhiteSpace(friendlyUrlName))
                query = query.Where(a => a.FriendlyUrlName.Contains(friendlyUrlName));

            if (!string.IsNullOrWhiteSpace(firstName))
                query = from aff in query
                         join addr in _addressRepository.Table on aff.AddressId equals addr.Id
                         where addr.FirstName.Contains(firstName)
                         select aff;

            if (!string.IsNullOrWhiteSpace(lastName))
                query = from aff in query
                         join addr in _addressRepository.Table on aff.AddressId equals addr.Id
                         where addr.LastName.Contains(lastName)
                         select aff;

            if (!showHidden)
                query = query.Where(a => a.Active);
            query = query.Where(a => !a.Deleted);

            if (loadOnlyWithOrders)
            {
                var ordersQuery = _orderRepository.Table;
                if (ordersCreatedFromUtc.HasValue)
                    ordersQuery = ordersQuery.Where(o => ordersCreatedFromUtc.Value <= o.CreatedOnUtc);
                if (ordersCreatedToUtc.HasValue)
                    ordersQuery = ordersQuery.Where(o => ordersCreatedToUtc.Value >= o.CreatedOnUtc);
                ordersQuery = ordersQuery.Where(o => !o.Deleted);

                query = from a in query
                        join o in ordersQuery on a.Id equals o.AffiliateId
                        select a;
            }

            query = query.Distinct().OrderByDescending(a => a.Id);

            var affiliates = new PagedList<Affiliate>(query, pageIndex, pageSize);

            return affiliates;
        }

        /// <summary>
        /// Inserts an affiliate
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual void InsertAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            _affiliateRepository.Insert(affiliate);

            //event notification
            _eventPublisher.EntityInserted(affiliate);
        }

        /// <summary>
        /// Updates the affiliate
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual void UpdateAffiliate(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            _affiliateRepository.Update(affiliate);

            //event notification
            _eventPublisher.EntityUpdated(affiliate);
        }

        /// <summary>
        /// Get full name
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>Affiliate full name</returns>
        public virtual string GetAffiliateFullName(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            var affiliateAddress = _addressService.GetAddressById(affiliate.AddressId);

            if (affiliateAddress == null)
                return string.Empty;

            var fullName = $"{affiliateAddress.FirstName} {affiliateAddress.LastName}";

            return fullName;
        }

        /// <summary>
        /// Generate affiliate URL
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>Generated affiliate URL</returns>
        public virtual string GenerateUrl(Affiliate affiliate)
        {
            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            var storeUrl = _webHelper.GetStoreLocation(false);
            var url = !string.IsNullOrEmpty(affiliate.FriendlyUrlName) ?
                //use friendly URL
                _webHelper.ModifyQueryString(storeUrl, NopAffiliateDefaults.AffiliateQueryParameter, affiliate.FriendlyUrlName) :
                //use ID
                _webHelper.ModifyQueryString(storeUrl, NopAffiliateDefaults.AffiliateIdQueryParameter, affiliate.Id.ToString());

            return url;
        }

        /// <summary>
        /// Validate friendly URL name
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <param name="friendlyUrlName">Friendly URL name</param>
        /// <returns>Valid friendly name</returns>
        public virtual string ValidateFriendlyUrlName(Affiliate affiliate, string friendlyUrlName)
        {
            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            //ensure we have only valid chars
            friendlyUrlName = _urlRecordService.GetSeName(friendlyUrlName, _seoSettings.ConvertNonWesternChars, _seoSettings.AllowUnicodeCharsInUrls);

            //max length
            //(consider a store URL + probably added {0}-{1} below)
            friendlyUrlName = CommonHelper.EnsureMaximumLength(friendlyUrlName, NopAffiliateDefaults.FriendlyUrlNameLength);

            //ensure this name is not reserved yet
            //empty? nothing to check
            if (string.IsNullOrEmpty(friendlyUrlName))
                return friendlyUrlName;
            //check whether such friendly URL name already exists (and that is not the current affiliate)
            var i = 2;
            var tempName = friendlyUrlName;
            while (true)
            {
                var affiliateByFriendlyUrlName = GetAffiliateByFriendlyUrlName(tempName);
                var reserved = affiliateByFriendlyUrlName != null && affiliateByFriendlyUrlName.Id != affiliate.Id;
                if (!reserved)
                    break;

                tempName = $"{friendlyUrlName}-{i}";
                i++;
            }

            friendlyUrlName = tempName;

            return friendlyUrlName;
        }

        #endregion

        #region Affiliate type 

        /// <summary>
        /// Inserts an affiliate type
        /// </summary>
        /// <param name="affiliateType">Affiliate</param>
        public virtual void InsertAffiliateType(AffiliateType affiliateType) {
            if(affiliateType == null)
                throw new ArgumentNullException(nameof(affiliateType));

            _affiliateTypeRepository.Insert(affiliateType);

            //event notification
            _eventPublisher.EntityInserted(affiliateType);
        }

        /// <summary>
        /// Updates the affiliate type
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual void UpdateAffiliateType(AffiliateType affiliateType) {
            if(affiliateType == null)
                throw new ArgumentNullException(nameof(affiliateType));

            _affiliateTypeRepository.Update(affiliateType);

            //event notification
            _eventPublisher.EntityUpdated(affiliateType);
        }

        /// <summary>
        /// Gets an affiliate type by affiliate type identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <returns>Affiliate</returns>
        public virtual AffiliateType GetAffiliateTypeById(int affiliateTypeId) {
            if(affiliateTypeId == 0)
                return null;

            return _affiliateTypeRepository.ToCachedGetById(affiliateTypeId);
        }

        /// <summary>
        /// Delete affiliate type
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        public virtual void DeleteAffiliateType(AffiliateType affiliateType) {
            if(affiliateType == null)
                throw new ArgumentNullException(nameof(affiliateType));

            affiliateType.Deleted = true;
            UpdateAffiliateType(affiliateType);

            //event notification
            _eventPublisher.EntityDeleted(affiliateType);
        }

        /// <summary>
        /// Get all affiliate types
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns></returns>
        public virtual IPagedList<AffiliateType> GetAllAffiliateTypes(int pageIndex, int pageSize) {
            var query = from p in _affiliateTypeRepository.Table
                        where p.Deleted == false
                        orderby p.Name
                        select p;

            return new PagedList<AffiliateType>(query, pageIndex, pageSize);
        }

        #endregion
    }
}