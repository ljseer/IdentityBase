﻿using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using IdentityBase.Models;
using IdentityBase.Public.EntityFramework.Interfaces;
using IdentityBase.Public.EntityFramework.Mappers;
using IdentityBase.Public.EntityFramework.Options;
using IdentityBase.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IdentityBase.Configuration;

namespace IdentityBase.Public.EntityFramework
{
    public class DefaultStoreInitializer : IStoreInitializer
    {
        private readonly EntityFrameworkOptions _options;
        private readonly ApplicationOptions _appOptions;
        private readonly ILogger<DefaultStoreInitializer> _logger;
        private readonly DefaultDbContext _defaultDbContext;
        private readonly IConfigurationDbContext _configurationDbContext;
        private readonly IPersistedGrantDbContext _persistedGrantDbContext;
        private readonly IUserAccountDbContext _userAccountDbContext;

        public DefaultStoreInitializer(
            EntityFrameworkOptions options,
            ApplicationOptions appOptions,
            ILogger<DefaultStoreInitializer> logger,
            DefaultDbContext defaultDbContext,
            IConfigurationDbContext configurationDbContext,
            IPersistedGrantDbContext persistedGrantDbContext,
            IUserAccountDbContext userAccountDbContext)
        {
            _options = options;
            _appOptions = appOptions; 
            _logger = logger;
            _defaultDbContext = defaultDbContext;
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _userAccountDbContext = userAccountDbContext;
        }

        public void InitializeStores()
        {
            // Only a leader may migrate or seed 
            if (_appOptions.Leader)
            {
                if (_options.MigrateDatabase)
                {
                    _logger.LogInformation("Try migrate database"); 
                    _defaultDbContext.Database.Migrate();
                }

                if (_options.SeedExampleData)
                {
                    _logger.LogInformation("Try seed initial data");
                    this.EnsureSeedData();
                }
            }
        }

        public void CleanupStores()
        {
            // Only leader may delete the database 
            if (_appOptions.Leader && _options.EnsureDeleted)
            {
                _logger.LogInformation("Ensure deleting database");
                _defaultDbContext.Database.EnsureDeleted(); 
            }
        }

        internal virtual void EnsureSeedData()
        {
            if (!_configurationDbContext.IdentityResources.Any())
            {
                var resources = JsonConvert.DeserializeObject<List<IdentityResource>>(
                    File.ReadAllText(Path.Combine(_options.SeedExampleDataPath, "data_resources_identity.json")));
                foreach (var resource in resources)
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                var resources = JsonConvert.DeserializeObject<List<ApiResource>>(
                    File.ReadAllText(Path.Combine(_options.SeedExampleDataPath, "data_resources_api.json")));
                foreach (var resource in resources)
                {
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.Clients.Any())
            {
                var clients = JsonConvert.DeserializeObject<List<Client>>(
                    File.ReadAllText(Path.Combine(_options.SeedExampleDataPath, "data_clients.json")));
                foreach (var client in clients)
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_userAccountDbContext.UserAccounts.Any())
            {
                var userAccounts = JsonConvert.DeserializeObject<List<UserAccount>>(
                    File.ReadAllText(Path.Combine(_options.SeedExampleDataPath, "data_users.json")));
                foreach (var userAccount in userAccounts)
                {
                    _userAccountDbContext.UserAccounts.Add(userAccount.ToEntity());
                }
                _userAccountDbContext.SaveChanges();
            }
        }
    }
}