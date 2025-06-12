package cargowise.eadaptorsamplewebclient;

public class Notification {
	private String message;
	private MessageType notificationType;
	
	public Notification(String message, MessageType notificationType) {
		this.message = message;
		this.notificationType = notificationType;
	}

	public String getMessage() {
		return message;
	}
	
	public MessageType getNotificationType() {
		return notificationType;
	}
	
	@Override
	public String toString() {
		return message;
	}
	
	public enum MessageType {
		Information,
		Warning,
		Error,
		Confirmation
	}
}
