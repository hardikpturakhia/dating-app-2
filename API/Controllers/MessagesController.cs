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
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
        {
            var username = User.GetUsername();
            if (username == createMessageDTO.RecipientUsername.ToLower())
                return BadRequest("You cannot send  messages to yourself");
            var sender = await _userRepository.GetUsersByUsernameAsync(username);
            var recipient = await _userRepository.GetUsersByUsernameAsync(createMessageDTO.RecipientUsername);
            if (recipient == null) return NotFound();
            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDTO.Content
            };
            _messageRepository.AddMessage(message);
            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDTO>(message));
            return BadRequest("Failed to send message");
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessagesForUser(
            [FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();
            var messages = await _messageRepository.GetMessageForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPage);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUsername();
            return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();
            var message = await _messageRepository.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();
            if (message.Sender.UserName == username) message.SenderDeleted = true;
            if (message.Recipient.UserName == username) message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }
            if (await _messageRepository.SaveAllAsync()) return Ok();
            return BadRequest("Problem deleting the message");
        }
    }
}