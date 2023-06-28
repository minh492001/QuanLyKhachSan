﻿using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using hotel_management_api.Common;
using hotel_management_api.Common.Enum;
using hotel_management_api.Database;
using hotel_management_api.Dto;
using hotel_management_api.Models;
using hotel_management_api.Repositories;
using hotel_management_api.Request;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace hotel_management_api.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        // private readonly IWebHostEnvironment _webHost;
        private readonly IMapper _mapper;
        public UserService(ApiOption apiOption, DatabaseContext databaseContext, IMapper mapper)
        {
            _userRepository = new UserRepository(apiOption, databaseContext, mapper);
            _mapper = mapper;
        }

        /// <summary>
        /// Get user
        /// </summary>
        /// <returns></returns>
        public object GetUsers(int limit, int page, string? name, string? email)
        {
            try
            {
                var query = this._userRepository.FindAll();
                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(row => row.Name.ToLower().Contains(name.ToLower()));
                }
                if (!string.IsNullOrEmpty(email))
                {
                    query = query.Where(row => row.Email.ToLower().Contains(email.ToLower()));
                }
                query.Skip((page - 1) * limit).Take(limit);
                var total = query.Count();
                int tmpByInt = total / limit;
                double tmpByDouble = (double)total / (double)limit;
                int totalPage = 1;
                if(tmpByDouble > (double)tmpByInt)
                {
                    totalPage = tmpByInt + 1;
                }
                else
                {
                    totalPage = tmpByInt;
                }
                query = query.Skip((page - 1) * limit).Take(limit);
                var amount = query.Count();
                return new
                {
                    data = query.ToList(),
                    Amount = amount,
                    PageSize = limit,
                    Total = total,
                    TotalPage = totalPage,
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="userStoreRequest"></param>
        /// <returns></returns>
        public object Store(UserStoreRequest userStoreRequest)
        {
            try
            {
                var userCheckList = _userRepository.FindAll().Where(row => row.Email == userStoreRequest.Email || row.NumberPhone == userStoreRequest.NumberPhone);
                if(userCheckList.Count() > 0)
                {
                    throw new Exception("Email or number phone already exist!");
                }
                var user = _mapper.Map<User>(userStoreRequest);
                user.Username = user.Email;
                user.Password = Untill.CreateMD5(user.Email);

                _userRepository.Create(user);
                _userRepository.SaveChange();

                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userStoreRequest"></param>
        /// <returns></returns>
        public object Update(int id, UserStoreRequest userStoreRequest)
        {
            try
            {
                var user = _userRepository.FindOrFail(id);
                if(user == null)
                {
                    throw new Exception("User does not exit!");
                }
                user.Name = userStoreRequest.Name;
                user.Email = userStoreRequest.Email;
                user.Address = userStoreRequest.Address;
                user.DateOfBirth = userStoreRequest.DateOfBirth;
                user.CitizenIdentification = userStoreRequest.CitizenIdentification;
                user.Role = userStoreRequest.Role;

                _userRepository.UpdateByEntity(user);
                _userRepository.SaveChange();
                return user;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object Delete(int id)
        {
            try
            {
                var user = _userRepository.FindOrFail(id);
                if (user == null)
                {
                    throw new Exception("User does not exit!");
                }

                _userRepository.DeleteByEntity(user);
                _userRepository.SaveChange();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
