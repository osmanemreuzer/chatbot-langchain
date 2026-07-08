import { useState } from "react";
import Sidebar from "../components/layout/Sidebar";
import ChatWindow from "../components/chat/ChatWindow";

export default function ChatPage() {
  const [activeConversationId, setActiveConversationId] = useState<string>("");
  const [activeTitle, setActiveTitle] = useState<string>("");

  const handleSelect = (id: string, title?: string) => {
    setActiveConversationId(id);
    setActiveTitle(title || "");
  };

  return (
    <div className="app-layout">
      <Sidebar
        activeId={activeConversationId}
        onSelect={handleSelect}
      />
      <main className="main-content">
        <ChatWindow
          conversationId={activeConversationId}
          conversationTitle={activeTitle}
        />
      </main>
    </div>
  );
}
