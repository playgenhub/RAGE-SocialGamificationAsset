﻿using System.Collections.Generic;
using System.Linq;
using PlayGen.SUGAR.Contracts.Shared;
using PlayGen.SUGAR.Data.Model;

namespace PlayGen.SUGAR.WebAPI.Extensions
{
    public static class RoleExtensions
    {
        public static RoleResponse ToContract(this Role roleModel)
        {
            if (roleModel == null)
            {
                return null;
            }

            return new RoleResponse
            {
                Id = roleModel.Id,
                Name = roleModel.Name
            };
        }

        public static IEnumerable<RoleResponse> ToContractList(this IEnumerable<Role> roleModels)
        {
            return roleModels.Select(ToContract).ToList();
        }

        public static Role ToModel(this RoleRequest roleContract)
        {
            return new Role
            {
                Name = roleContract.Name
            };
        }

    }
}