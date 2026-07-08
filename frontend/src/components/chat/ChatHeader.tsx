import { Bot, MoreVertical } from "lucide-react";

interface Props {
  title: string;
}

export default function ChatHeader({ title }: Props) {
  return (
    <div className="chat-header">
      <div className="chat-header-avatar">
        <Bot size={22} />
      </div>
      <div className="chat-header-info">
        <div className="chat-header-name">{title || "Yeni Sohbet"}</div>
        <div className="chat-header-status">çevrimiçi</div>
      </div>
      <button className="btn-icon">
        <MoreVertical size={20} />
      </button>
    </div>
  );
}
