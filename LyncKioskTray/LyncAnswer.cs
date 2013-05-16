using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;

namespace LyncKioskTray
{
    public class LyncAnswer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        private readonly LyncClient _lyncClient;

        public LyncAnswer(LyncClient lyncClient)
        {
            _lyncClient = lyncClient;
        }

        public void Start()
        {
            AddHandlersToExistingConversations();
        }

        private void AddHandlersToExistingConversations()
        {
            _lyncClient.ConversationManager.ConversationAdded += ConversationAdded;

            try
            {
                foreach (var existingConversation in _lyncClient.ConversationManager.Conversations)
                {
                    AcceptVideoWhenVideoAdded(existingConversation);
                    GoFullScreenWhenVideoAdded(existingConversation);
                    Log.DebugFormat("Existing conversation with '{0}' found",
                                    string.Join(",", existingConversation.Participants.Select(x => x.Contact.Uri)));
                }
            }
            catch (ClientNotFoundException)
            {
                //No problem, if we can't find the client, there won't be any conversations to watch
            }
        }

        private static void ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            try
            {
                var avModality = e.Conversation.Modalities[ModalityTypes.AudioVideo];

                //Save the video state so that we avoid wacky things when it changes
                var avModalityState = avModality.State;
                Log.DebugFormat("Conversation Added, AV Modality: {0}", avModalityState);

                //Check if the new conversation is a new incoming video request
                if (avModalityState == ModalityState.Notified)
                    AnswerVideo(e.Conversation);

                AcceptVideoWhenVideoAdded(e.Conversation);
                GoFullScreenWhenVideoAdded(e.Conversation);
            }
            catch (Exception ex)
            {
                Log.Error("Error in ConversationAdded", ex);
            }
        }

        private static void AcceptVideoWhenVideoAdded(Conversation conversation)
        {
            var avModality = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];

            avModality.ModalityStateChanged += (o, args) =>
            {
                try
                {
                    var newState = args.NewState;
                    Log.DebugFormat("Conversation Modality State Changed to '{0}'", newState);

                    if (newState == ModalityState.Notified)
                        AnswerVideo(conversation);
                    if (newState == ModalityState.Connected)
                        StartOurVideo(avModality);
                }
                catch (Exception ex)
                {
                    Log.Error("Error handling modality state change", ex);
                }
            };
        }

        private static void GoFullScreenWhenVideoAdded(Conversation conversation)
        {
            var avModality = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];

            avModality.ModalityStateChanged += (o, args) =>
            {
                try
                {
                    var newState = args.NewState;

                    if (newState == ModalityState.Notified)
                    {
                        var lync = LyncClient.GetAutomation();
                        var win = lync.GetConversationWindow(conversation);
                        //win.MoveAndResize(0, 0, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
                        win.ShowFullScreen(0);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error handling modality state change", ex);
                }
            };
        }

        private static void StartOurVideo(AVModality avModality)
        {
            var channelStream = avModality.VideoChannel;

            while (!channelStream.CanInvoke(ChannelAction.Start))
            {
            }

            channelStream.BeginStart(ar => { }, channelStream);
            var count = 0;
            while ((channelStream.State != ChannelState.SendReceive) && (count < 5))
            {
                Thread.Sleep(1000);

                try
                {
                    channelStream.BeginStart(ar => { }, channelStream);
                }
                catch (NotSupportedException)
                {
                    //This is normal...
                }
                count++;
            }
        }

        private static void AnswerVideo(Conversation conversation)
        {
            var converstationState = conversation.State;
            if (converstationState == ConversationState.Terminated)
            {
                return;
            }

            var av = (AVModality)conversation.Modalities[ModalityTypes.AudioVideo];
            if (av.CanInvoke(ModalityAction.Connect))
            {
                av.Accept();

                // Get ready to be connected, then WE can start OUR video
                //av.ModalityStateChanged += AVModality_ModalityStateChanged;
            }
            else
            {
                Log.Warn("Unable to start video do to 'CanInvoke' being false");
            }
        }

    }
}
