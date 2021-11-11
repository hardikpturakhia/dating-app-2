using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.DTOs;
using API.Extentions;
using API.Entities;
using AutoMapper;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public MessagesController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUsername();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                return BadRequest("You cannot send  messages to yourself");
            var sender = await _unitOfWork.UserRepository.GetUsersByUsernameAsync(username);
            var recipient = await _unitOfWork.UserRepository.GetUsersByUsernameAsync(createMessageDTO.RecipientUsername);
            if (recipient == null) return NotFound();
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };
            _unitOfWork.MessageRepository.AddMessage(message);
            if (await _unitOfWork.Complete()) return Ok(_mapper.Map<MessageDTO>(message));
            return BadRequest("Failed to send message");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser(
            [FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _unitOfWork.MessageRepository.GetMessageForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPage);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();
            return Ok(await _unitOfWork.MessageRepository.GetMessageThread(currentUsername, username));
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _unitOfWork.MessageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            if (message.Recipient.UserName == username) message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }
            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("Problem deleting the message");
        }
    }
}