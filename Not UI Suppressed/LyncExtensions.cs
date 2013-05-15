using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;

namespace SuperSimpleLyncKiosk
{
    public static class LyncExtensions
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Task StartAsync(this VideoChannel videoChannel)
        {
            return Task.Factory.FromAsync(videoChannel.BeginStart, videoChannel.EndStart, null);
        }

        public static void AutoAnswerIncomingVideoCalls(this ConversationManager conversationManager)
        {
            conversationManager.ConversationAdded += ConversationManager_ConversationAdded;
            foreach (var existingConversation in conversationManager.Conversations)
            {
                AcceptVideoWhenVideoAdded(existingConversation);
                GoFullScreenWhenVideoAdded(existingConversation);
                Log.DebugFormat("Existing conversation with '{0}' found",
                                string.Join(",", existingConversation.Participants.Select(x => x.Contact.Uri)));
            }
        }

        public static void AnswerVideo(this Conversation conversation)
        {
            var converstationState = conversation.State;
            if (converstationState == ConversationState.Terminated)
            {
                return;
            }

            var av = (AVModality) conversation.Modalities[ModalityTypes.AudioVideo];
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

        private static void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            try
            {
                var avModality = e.Conversation.Modalities[ModalityTypes.AudioVideo];

                //Save the video state so that we avoid wacky things when it changes
                var avModalityState = avModality.State;
                Log.DebugFormat("Conversation Added, AV Modality: {0}", avModalityState);

                //Check if the new conversation is a new incoming video request
                if (avModalityState == ModalityState.Notified)
                    e.Conversation.AnswerVideo();

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
            var avModality = (AVModality) conversation.Modalities[ModalityTypes.AudioVideo];
            
            avModality.ModalityStateChanged += (o, args) =>
                {
                    try
                    {
                        var newState = args.NewState;
                        Log.DebugFormat("Conversation Modality State Changed to '{0}'", newState);

                        if (newState == ModalityState.Notified)
                            conversation.AnswerVideo();
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

                channelStream.BeginStart(ar => { }, channelStream);
                count++;
            }
        }
    }
}
