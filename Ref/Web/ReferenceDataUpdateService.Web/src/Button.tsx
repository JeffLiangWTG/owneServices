import React from 'react';

interface IButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
	onClick: () => void;
}

const Button = ({ onClick, ...props }: IButtonProps) => {
	const handleClick = (e: React.MouseEvent<HTMLButtonElement, MouseEvent>) => {
		if (!props.disabled && !props.hidden && e.button === 0) {
			if (document.activeElement instanceof HTMLElement) {
				document.activeElement.blur();
			}
			onClick();
		}
	}
	return (
		<button
		{...props}
			onMouseDown={handleClick}
		>{props.children}
		</button>
	);
};

export default Button;
