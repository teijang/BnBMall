﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Payments.PaymentWall
{
    public abstract class Paymentwall_Base
    {

        /*
         * Paymentwall library version
         */
        const string LIB_VERSION = "1.0.0";

        /*
         * URLs for Paymentwall Pro
         */
        protected const string CHARGE_URL = "https://api.paymentwall.com/api/pro/v1/charge";
        protected const string SUBS_URL = "https://api.paymentwall.com/api/pro/v1/subscription";

        /*
         * API types
         */
        public const int API_VC = 1;
        public const int API_GOODS = 2;
        public const int API_CART = 3;

        /*
         * Controllers for APIs
         */
        protected const string CONTROLLER_PAYMENT_VIRTUAL_CURRENCY = "ps";
        protected const string CONTROLELR_PAYMENT_DIGITAL_GOODS = "subscription";
        protected const string CONTROLLER_PAYMENT_CART = "cart";

        /**
	     * Signature versions
	     */
        protected const int DEFAULT_SIGNATURE_VERSION = 3;
        protected const int SIGNATURE_VERSION_1 = 1;
        protected const int SIGNATURE_VERSION_2 = 2;
        protected const int SIGNATURE_VERSION_3 = 3;

        protected List<string> errors = new List<string>();

        /**
         * Paymentwall API type
         * @param int apiType
         */
        public static int apiType;

        /**
         * Paymentwall application key - can be found in your merchant area
         * @param string appKey
         */
        public static string appKey;

        /**
         * Paymentwall secret key - can be found in your merchant area
         * @param string secretKey
         */
        public static string secretKey;

        /**
         * Paymentwall Pro API Key
         * @param string proApiKey
         */
        public static string proApiKey;

        /*
         * @param int apiType API type
         */
        public static void setApiType(int apiType)
        {
            Paymentwall_Base.apiType = apiType;
        }

        public static int getApiType()
        {
            return Paymentwall_Base.apiType;
        }

        /*
         * @param string appKey application key of your application, can be found inside of your Paymentwall Merchant Account
         */
        public static void setAppKey(string appKey)
        {
            Paymentwall_Base.appKey = appKey;
        }

        public static string getAppKey()
        {
            return Paymentwall_Base.appKey;
        }

        /*
         *  @param string secretKey secret key of your application, can be found inside of your Paymentwall Merchant Account
         */
        public static void setSecretKey(string secretKey)
        {
            Paymentwall_Base.secretKey = secretKey;
        }

        public static string getSecretKey()
        {
            return Paymentwall_Base.secretKey;
        }

        /*
         * @param string proApiKey API key used for Pro authentication
         */
        public static void setProApiKey(string proApiKey)
        {
            Paymentwall_Base.proApiKey = proApiKey;
        }

        public static string getProApiKey()
        {
            return Paymentwall_Base.proApiKey;
        }

        /*
         * Fill the array with the errors found at execution
         * 
         * @param string err
         * @return int
         */
        protected int appendToErrors(string err)
        {
            this.errors.Add(err);
            return this.errors.Count;
        }

        /**
         * Return errors
         * 
         * @return List<string>
         */
        public List<string> getErrors()
        {
            return this.errors;
        }

        /*
         * Return error summary
         * 
         * @return string
         */
        public string getErrorSummary()
        {
            return string.Join("\n", this.getErrors());
        }

    }
}