package cargowise.eadaptorsamplewebclient;

import java.awt.Color;
import java.awt.Component;
import java.awt.Container;
import java.awt.Dimension;
import java.awt.EventQueue;
import java.awt.FocusTraversalPolicy;
import java.awt.Font;
import java.awt.SystemColor;
import java.awt.Toolkit;
import java.awt.event.ActionListener;
import java.awt.event.ActionEvent;
import java.util.Vector;

import javax.swing.DefaultListCellRenderer;
import javax.swing.DefaultListModel;
import javax.swing.GroupLayout;
import javax.swing.GroupLayout.Alignment;
import javax.swing.JButton;
import javax.swing.JFileChooser;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JList;
import javax.swing.JPanel;
import javax.swing.JScrollPane;
import javax.swing.JTextField;
import javax.swing.LayoutStyle.ComponentPlacement;
import javax.swing.UIManager;
import javax.swing.border.EmptyBorder;
import javax.swing.border.EtchedBorder;
import javax.swing.filechooser.FileNameExtensionFilter;

import com.cargowise.eservices.common.FaultException;

public class eAdaptorSampleWebClientView extends JFrame {
	private static final long serialVersionUID = 4527499970255672326L;
	private static final int FONT_SIZE = 22;
	private static final String FONT_NAME = "Microsoft Sans Serif"; 

	private JPanel contentPane;
	private JButton btnPing;
	private JButton btnBrowse;
	private JButton btnSend;

	private DefaultListModel<Notification> notificationList;
	private JList<Notification> listForScrollPane;
	private JScrollPane scrollPane;
	private JFileChooser fileChooser;

	private JTextField tfServiceAddress;
	private JTextField tfMessageFileName;
	private JTextField tfRecipientId;
	private JTextField tfSenderId;
	private JTextField tfPassword;

	/**
	 * Launch the application.
	 */
	public static void main(String[] args) {
		try {
			UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
		} catch (Throwable e) {
			e.printStackTrace();
		}
		EventQueue.invokeLater(new Runnable() {
			public void run() {
				try {
					eAdaptorSampleWebClientView frame = new eAdaptorSampleWebClientView();
					frame.setVisible(true);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
		});
	}

	/**
	 * Create the frame.
	 */
	public eAdaptorSampleWebClientView() {
		setIconImage(Toolkit.getDefaultToolkit().getImage(eAdaptorSampleWebClientView.class.getResource("/Logo/logo.png")));
		setMinimumSize(new Dimension(1280, 960));
		setTitle("eAdaptor Sample Client");
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		setBounds(100, 100, 958, 708);
		
		contentPane = new JPanel();
		contentPane.setBorder(new EmptyBorder(5, 5, 5, 5));
		setContentPane(contentPane);
		
		Font font = new Font(FONT_NAME, Font.PLAIN, FONT_SIZE);
		
		JLabel lblServiceAddress = new JLabel("Service Address");
		lblServiceAddress.setFont(font);

		tfServiceAddress = new JTextField();
		tfServiceAddress.setFont(font);
		tfServiceAddress.setColumns(10);

		btnPing = new JButton("Ping");
		btnPing.setFont(font);
		btnPing.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				btnPing_Click();
			}
		});

		JLabel lblMessageFile = new JLabel("Message File");
		lblMessageFile.setFont(font);

		tfMessageFileName = new JTextField();
		tfMessageFileName.setEditable(false);
		tfMessageFileName.setFont(font);
		tfMessageFileName.setBackground(SystemColor.control);
		tfMessageFileName.setColumns(10);

		btnBrowse = new JButton("Browse");
		btnBrowse.setFont(font);
		btnBrowse.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				btnBrowse_Click();
			}
		});
		
		fileChooser = new JFileChooser();
		fileChooser.setFileFilter(new FileNameExtensionFilter("XmlInterchange files|*.xml", "xml"));

		JLabel lblRecipientId = new JLabel("Recipient Id");
		lblRecipientId.setFont(font);

		JLabel lblSenderId = new JLabel("Sender Id");
		lblSenderId.setFont(font);

		JLabel lblPassword = new JLabel("Password");
		lblPassword.setFont(font);

		tfRecipientId = new JTextField();
		tfRecipientId.setFont(font);
		tfRecipientId.setColumns(10);

		tfSenderId = new JTextField();
		tfSenderId.setFont(font);
		tfSenderId.setText("Sender Id");
		tfSenderId.setColumns(10);

		tfPassword = new JTextField();
		tfPassword.setFont(font);
		tfPassword.setText("Password");
		tfPassword.setColumns(10);

		btnSend = new JButton("Send");
		btnSend.setFont(new Font(FONT_NAME, Font.PLAIN, FONT_SIZE + 10));
		btnSend.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent e) {
				btnSend_Click();
			}
		});
		
		Vector<Component> order = new Vector<Component>();
		order.add(tfServiceAddress);
		order.add(btnPing);
		order.add(btnBrowse);
		order.add(tfRecipientId);
		order.add(tfSenderId);
		order.add(tfPassword);
		order.add(btnSend);
		CustomFocusTraversalPolicy focusPolicy = new CustomFocusTraversalPolicy(order);
		setFocusTraversalPolicy(focusPolicy);

		notificationList = new DefaultListModel<Notification>();
		listForScrollPane = new JList<Notification>(notificationList);
		listForScrollPane.setFont(new Font(FONT_NAME, Font.BOLD, FONT_SIZE));
		listForScrollPane.setCellRenderer(new NotificationListRenderer());
		scrollPane = new JScrollPane(listForScrollPane);
		scrollPane.setBorder(new EtchedBorder(EtchedBorder.LOWERED, null, null));

		GroupLayout gl_contentPane = new GroupLayout(contentPane);
		
		gl_contentPane.setHorizontalGroup(
			gl_contentPane.createParallelGroup(Alignment.LEADING)
				.addGroup(gl_contentPane.createSequentialGroup()
					.addContainerGap()
					.addGroup(gl_contentPane.createParallelGroup(Alignment.LEADING)
						.addComponent(scrollPane, Alignment.LEADING)
						.addGroup(gl_contentPane.createSequentialGroup()
							.addGroup(gl_contentPane.createParallelGroup(Alignment.LEADING)
								.addComponent(lblServiceAddress)
								.addComponent(lblMessageFile)
								.addComponent(lblRecipientId)
								.addComponent(lblSenderId)
								.addComponent(lblPassword))
							.addPreferredGap(ComponentPlacement.RELATED)
							.addGroup(gl_contentPane.createParallelGroup(Alignment.LEADING)
								.addGroup(gl_contentPane.createSequentialGroup()
									.addPreferredGap(ComponentPlacement.RELATED)
									.addGroup(gl_contentPane.createParallelGroup(Alignment.LEADING)
										.addComponent(tfServiceAddress, GroupLayout.DEFAULT_SIZE, 870, Short.MAX_VALUE)
										.addComponent(tfMessageFileName, GroupLayout.DEFAULT_SIZE, 870, Short.MAX_VALUE))
									.addPreferredGap(ComponentPlacement.RELATED)
									.addGroup(gl_contentPane.createParallelGroup(Alignment.LEADING)
										.addComponent(btnBrowse, GroupLayout.PREFERRED_SIZE, 150, GroupLayout.PREFERRED_SIZE)
										.addComponent(btnPing, GroupLayout.PREFERRED_SIZE, 150, GroupLayout.PREFERRED_SIZE)))
								.addGroup(gl_contentPane.createSequentialGroup()
									.addGroup(gl_contentPane.createParallelGroup(Alignment.LEADING, false)
										.addComponent(tfSenderId, GroupLayout.DEFAULT_SIZE, 456, GroupLayout.PREFERRED_SIZE)
										.addComponent(tfPassword, GroupLayout.DEFAULT_SIZE, 456, GroupLayout.PREFERRED_SIZE)
										.addComponent(tfRecipientId, GroupLayout.DEFAULT_SIZE, 456, GroupLayout.PREFERRED_SIZE))
									.addPreferredGap(ComponentPlacement.RELATED)
									.addComponent(btnSend, GroupLayout.PREFERRED_SIZE, 214, GroupLayout.PREFERRED_SIZE)))))
					.addContainerGap())
		);
			
		gl_contentPane.setVerticalGroup(
			gl_contentPane.createParallelGroup(Alignment.LEADING)
				.addGroup(gl_contentPane.createSequentialGroup()
					.addGap(12)
					.addGroup(gl_contentPane.createParallelGroup(Alignment.BASELINE)
						.addComponent(lblServiceAddress, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE)
						.addComponent(tfServiceAddress, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE)
						.addComponent(btnPing, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE))
					.addPreferredGap(ComponentPlacement.RELATED)
					.addGroup(gl_contentPane.createParallelGroup(Alignment.BASELINE)
						.addComponent(lblMessageFile, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE)
						.addComponent(tfMessageFileName, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE)
						.addComponent(btnBrowse, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE))
					.addPreferredGap(ComponentPlacement.RELATED)
					.addGroup(gl_contentPane.createParallelGroup(Alignment.LEADING, false)
						.addGroup(gl_contentPane.createSequentialGroup()
							.addGroup(gl_contentPane.createParallelGroup(Alignment.BASELINE)
								.addComponent(lblRecipientId, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE)
								.addComponent(tfRecipientId, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE))
							.addPreferredGap(ComponentPlacement.RELATED)
							.addGroup(gl_contentPane.createParallelGroup(Alignment.TRAILING)
								.addComponent(lblSenderId, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE)
								.addComponent(tfSenderId, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE))
							.addPreferredGap(ComponentPlacement.RELATED)
							.addGroup(gl_contentPane.createParallelGroup(Alignment.TRAILING)
								.addComponent(lblPassword, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE)
								.addComponent(tfPassword, GroupLayout.PREFERRED_SIZE, 40, GroupLayout.PREFERRED_SIZE)))
						.addComponent(btnSend, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE, Short.MAX_VALUE))
					.addPreferredGap(ComponentPlacement.UNRELATED)
					.addComponent(scrollPane)
					.addContainerGap())
		);

		contentPane.setLayout(gl_contentPane);
	}

	private void btnPing_Click() {
		if (!validateSenderId()) {
			return;
		}
		if (!validatePassword()) {
			return;
		}

		validateServiceAddress();
		addNotification(new Notification("", Notification.MessageType.Information));
	}

	private void btnBrowse_Click() {
		int returnValue = fileChooser.showOpenDialog(eAdaptorSampleWebClientView.this);
		if (returnValue == JFileChooser.APPROVE_OPTION) {
			tfMessageFileName.setText(fileChooser.getSelectedFile().getAbsolutePath());
		}
	}

	private void btnSend_Click() {
		if (!validateMessageFileName()) {
			return;
		}
		if (!validateRecipientId()) {
			return;
		}
		if (!validateSenderId()) {
			return;
		}
		if (!validatePassword()) {
			return;
		}
		if (!validateServiceAddress(true)) {
			return;
		}

		String notificationMessage = "Send Message " + tfMessageFileName.getText().trim() + " to " + tfRecipientId.getText().trim();
		addNotification(new Notification(notificationMessage, Notification.MessageType.Information));

		try {
			eAdaptorSampleWebClient.sendMessage(
				tfServiceAddress.getText().trim(),
				tfMessageFileName.getText().trim(),
				tfRecipientId.getText().trim(),
				tfSenderId.getText().trim(),
				tfPassword.getText()
			);
			addNotification(new Notification("Send message successful.", Notification.MessageType.Confirmation));
		} catch (Exception ex) {
			addNotification(new Notification("Error during message sending: " + ex.getMessage(), Notification.MessageType.Error));
			Throwable cause = ex.getCause();
			if (cause != null && cause instanceof FaultException) {
				addNotification(new Notification(
					((FaultException)cause).getXMLNode(),
					Notification.MessageType.Error
				));
			}
		}

		addNotification(new Notification("", Notification.MessageType.Information));
	}

	private boolean validateSenderId() {
		if (tfSenderId.getText().trim().isEmpty()) {
			addNotification(new Notification("Please specify Sender Id", Notification.MessageType.Warning));
			return false;
		}
		return true;
	}

	private boolean validateRecipientId() {
		String recipientId = tfRecipientId.getText().trim();
		if (recipientId.isEmpty()) {
			addNotification(new Notification("Please specify Recipient Id", Notification.MessageType.Warning));
			return false;
		}
		if (recipientId.length() != 9) {
			addNotification(new Notification("Recipient Id should be a 9-character code", Notification.MessageType.Warning));
			return false;
		}
		return true;
	}

	private boolean validatePassword() {
		return true;
	}

	private boolean validateServiceAddress() {
		return validateServiceAddress(false);
	}

	private boolean validateServiceAddress(boolean suppressSuccessMessages) {
		String serviceAddress = tfServiceAddress.getText().trim();
		if (serviceAddress.isEmpty()) {
			addNotification(new Notification("Please specify Service Address", Notification.MessageType.Warning));
			return false;
		}

		if (!suppressSuccessMessages) {
			addNotification(new Notification("Ping " + serviceAddress, Notification.MessageType.Information));
		}

		try {
			if (eAdaptorSampleWebClient.ping(serviceAddress, tfSenderId.getText().trim(), tfPassword.getText())) {
				if (!suppressSuccessMessages) {
					addNotification(new Notification("Ping successful.", Notification.MessageType.Confirmation));
				}
				return true;
			} else {
				addNotification(new Notification("Ping error. Use another address or check Service is available", Notification.MessageType.Error));
				return false;
			}
		} catch (Exception ex) {
			addNotification(new Notification("Ping error: " + ex.getMessage(), Notification.MessageType.Error));
			return false;
		}
	}

	private boolean validateMessageFileName() {
		if (tfMessageFileName.getText().trim().isEmpty()) {
			addNotification(new Notification("Please select Message FileName", Notification.MessageType.Warning));
			return false;
		}
		return true;
	}

	private void addNotification(Notification notification) {
		notificationList.addElement(notification);
	}
	
	private class NotificationListRenderer extends DefaultListCellRenderer {
		private static final long serialVersionUID = -1197341934290960787L;
		
		@Override
		public Component getListCellRendererComponent(JList<?> list, Object value, int index, boolean isSelected, boolean cellHasFocus) {
			Notification.MessageType notificationType = ((Notification)value).getNotificationType();
			
			Color fontColor = Color.DARK_GRAY;
			switch (notificationType) {
				case Error:
					fontColor = Color.RED.darker();
					break;
				case Warning:
					fontColor = Color.ORANGE.darker();
					break;
				case Confirmation:
					fontColor = Color.GREEN.darker();
					break;
				default:
					break;
			}
			
			Component component = super.getListCellRendererComponent(list, value, index, isSelected, cellHasFocus);
			component.setForeground(fontColor);
			return this;
		}
	}
	
	private class CustomFocusTraversalPolicy extends FocusTraversalPolicy {
        private Vector<Component> order;
        
        public CustomFocusTraversalPolicy(Vector<Component> order) {
            this.order = new Vector<Component>(order.size());
            this.order.addAll(order);
        }
        
        public Component getComponentAfter(Container focusCycleRoot, Component aComponent) {
            int idx = (order.indexOf(aComponent) + 1) % order.size();
            return order.get(idx);
        }
 
        public Component getComponentBefore(Container focusCycleRoot, Component aComponent) {
            int idx = order.indexOf(aComponent) - 1;
            if (idx < 0) {
                idx = order.size() - 1;
            }
            return order.get(idx);
        }
 
        public Component getDefaultComponent(Container focusCycleRoot) {
            return order.get(0);
        }
 
        public Component getLastComponent(Container focusCycleRoot) {
            return order.lastElement();
        }
 
        public Component getFirstComponent(Container focusCycleRoot) {
            return order.get(0);
        }
	}
}
